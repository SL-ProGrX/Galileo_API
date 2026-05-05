using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAdvertenciasRegistroDB
    {

        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCoAdvertenciasRegistroDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Inserta o actualiza un registro de advertiencia 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<int> CoAdvertenciasRegistro_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {
            if (datos is null)
            {
                return DbHelper.CreateErrorResponse<int>("Los datos de la advertencia son requeridos.", -2, 0);
            }

            var movimiento = datos.linea == 0 ? "Registra - WEB" : "Modifica - WEB";
            var parametros = CrearParametrosRegistro(datos, usuario);

            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                "EXEC dbo.spCbrAdvertenciaRegistro @cedula, @cod_advertencia, @fecha_vence, @usuario, @notas, @linea",
                0,
                parametros);

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<int>(result.Description ?? "Error al guardar advertencia.", result.Code ?? -1, 0);
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Advertencia ..:Id.({result.Result} ) Cod.({NormalizarCodigo(datos.cod_advertencia)} ) Ced.{NormalizarTexto(datos.cedula)}",
                movimiento);

            return result;
        }
        

        /// <summary>
        /// Consulta el listado de advertencias para una cedula 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="cod_advertencia"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAdvertenciasRegistroData>> CoAdvertenciasRegistro_Consultar(int CodEmpresa, string cedula, string cod_advertencia, int linea)
        
        {
            var ced = NormalizarTexto(cedula);
            var cod = NormalizarCodigo(cod_advertencia);
            var consulta = CrearConsultaRegistro(cod);

            return DbHelper.ExecuteListQuery<CoAdvertenciasRegistroData>(
                new PortalDB(_config),
                CodEmpresa,
                consulta,
                new { cedula = ced, cod_advertencia = cod, linea });
        }


        /// <summary>
        /// Elimina el registro de una advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cedula"></param>
        /// <param name="cod_advertencia"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto CoAdvertenciasRegistro_Delete(int CodEmpresa, string usuario, string cedula, string cod_advertencia, int linea)
        {
            var ced = NormalizarTexto(cedula);
            var cod = NormalizarCodigo(cod_advertencia);

            if (string.IsNullOrWhiteSpace(ced) || string.IsNullOrWhiteSpace(cod) || linea <= 0)
            {
                return DbHelper.ErrorResponse("Datos incompletos para eliminar advertencia.", -2);
            }

            const string query = @"
                    DELETE FROM dbo.CBR_ADVERTENCIAS_CASOS
                    WHERE Cedula = @cedula
                      AND UPPER(RTRIM(cod_Advertencia)) = @cod_advertencia
                      AND Linea = @linea;";

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { cedula = ced, cod_advertencia = cod, linea });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Advertencia ..: Id. {linea} Cod. {cod} Ced. {ced}",
                "Elimina - WEB");

            return result;
        }


        /// <summary>
        /// Consulta el ultimo o el primer codigo de advertencia existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_advertencia"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> CoAdvertenciasRegistro_TipoAdvertencia(int CodEmpresa, string cod_advertencia, int orden)
        {
            var cod = NormalizarCodigo(cod_advertencia);
            var query = CrearSqlTipoAdvertencia(orden);

            var result = DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new DropDownListaGenericaModel(),
                new { cod_advertencia = cod });

            return new ErrorDto<DropDownListaGenericaModel>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new DropDownListaGenericaModel()
            };
        }


        /// <summary>
        /// Consulta el listado de advertencias existentes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        RTRIM(cod_Advertencia) AS item,
                        RTRIM(Descripcion)     AS descripcion
                    FROM dbo.CBR_ADVERTENCIAS_TIPO
                    WHERE ISNULL(Activa,0) = 1
                    ORDER BY cod_Advertencia;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }


        /// <summary>
        /// Consulta el lista de socios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAdvertenciasRegistroSociosData>> CoAdvertenciasRegistroSocios_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        cedula  AS cedula_colilla,
                        cedular AS cedula_real,
                        nombre
                    FROM dbo.SOCIOS
                    ORDER BY nombre;";

            return DbHelper.ExecuteListQuery<CoAdvertenciasRegistroSociosData>(new PortalDB(_config), CodEmpresa, query);
        }


        /// <summary>
        /// Consulta el nombre de un socio por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<string> CoAdvertenciasRegistroNombreSocios_Consultar(int CodEmpresa, string cedula)
        {
            const string query = @"SELECT TOP 1 nombre FROM dbo.socios WHERE cedula = @cedula;";
            var ced = NormalizarTexto(cedula);

            var result = DbHelper.ExecuteSingleQuery(new PortalDB(_config), CodEmpresa, query, string.Empty, new { cedula = ced });

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }


        /// <summary>
        /// Aplica la resolucion de una advertencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CoAdvertenciasRegistroResolucion_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {
            if (datos is null)
            {
                return DbHelper.ErrorResponse("Los datos de resolución son requeridos.", -2);
            }

            var ced = NormalizarTexto(datos.cedula);
            var cod = NormalizarCodigo(datos.cod_advertencia);

            if (string.IsNullOrWhiteSpace(ced) || string.IsNullOrWhiteSpace(cod) || datos.linea <= 0)
            {
                return DbHelper.ErrorResponse("Datos incompletos para guardar resolución.", -2);
            }

            const string query = @"
                    UPDATE dbo.CBR_ADVERTENCIAS_CASOS
                    SET
                        Estado = @estado,
                        Resolucion_Fecha = dbo.MyGetdate(),
                        Resolucion_Usuario = @usuario,
                        Resolucion_Notas = @resolucion_notas
                    WHERE UPPER(RTRIM(cod_Advertencia)) = @cod_advertencia
                      AND Linea = @linea
                      AND Cedula = @cedula;";

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new
                {
                    datos.estado,
                    datos.linea,
                    cod_advertencia = cod,
                    cedula = ced,
                    datos.resolucion_notas,
                    usuario
                });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Advertencia ..:Id.({datos.linea} ) Cod.({cod} ) Ced.{ced} Est.:{datos.estado}",
                "Registra - WEB");

            return result;
        }

        private static string CrearConsultaRegistro(string codAdvertencia)
        {
            var filtroAdvertencia = string.IsNullOrWhiteSpace(codAdvertencia)
                ? string.Empty
                : "AND UPPER(RTRIM(Cs.cod_Advertencia)) = @cod_advertencia AND Cs.Linea = @linea";

            return $@"
                    SELECT
                        Cs.*,
                        Tp.Descripcion AS advertenciad_desc
                    FROM dbo.CBR_ADVERTENCIAS_CASOS Cs
                    INNER JOIN dbo.CBR_ADVERTENCIAS_TIPO Tp
                        ON Cs.cod_Advertencia = Tp.cod_Advertencia
                    WHERE Cs.Cedula = @cedula
                    {filtroAdvertencia}
                    ORDER BY Cs.Estado, Cs.Registro_Fecha DESC;";
        }

        private static string CrearSqlTipoAdvertencia(int orden)
        {
            var operador = orden == 1 ? ">" : "<";
            var ordenSql = orden == 1 ? "ASC" : "DESC";

            return $@"
                    SELECT TOP 1
                        RTRIM(COD_ADVERTENCIA) AS item,
                        RTRIM(descripcion)     AS descripcion
                    FROM dbo.CBR_ADVERTENCIAS_TIPO
                    WHERE UPPER(RTRIM(cod_Advertencia)) {operador} @cod_advertencia
                      AND ISNULL(Activa,0) = 1
                    ORDER BY cod_Advertencia {ordenSql};";
        }

        private static object CrearParametrosRegistro(CoAdvertenciasRegistroData datos, string usuario)
        {
            return new
            {
                linea = datos.linea,
                cod_advertencia = NormalizarCodigo(datos.cod_advertencia),
                cedula = NormalizarTexto(datos.cedula),
                datos.fecha_vence,
                datos.notas,
                usuario
            };
        }

        private static string NormalizarCodigo(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpper();
        }

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
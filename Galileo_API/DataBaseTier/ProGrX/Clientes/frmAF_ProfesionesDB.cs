using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFProfesionesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string CodigoNuevo = "Nuevo";

        private const string SqlProfesionesTotal = @"
                    SELECT COUNT(cod_profesion)
                    FROM dbo.afi_profesiones
                    WHERE @hasFilter = 0 OR
                          cod_profesion LIKE @filtro OR
                          descripcion LIKE @filtro;";

        private const string SqlProfesionesLista = @"
                    SELECT cod_profesion AS item,
                           descripcion
                    FROM dbo.afi_profesiones
                    WHERE @hasFilter = 0 OR
                          cod_profesion LIKE @filtro OR
                          descripcion LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_profesion END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_profesion END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        cod_profesion ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlProfesionInsert = @"
                    INSERT INTO dbo.afi_profesiones
                    (
                        descripcion
                    )
                    VALUES
                    (
                        @Descripcion
                    );
                    SELECT ISNULL(MAX(cod_profesion), 0) AS Ultimo
                    FROM dbo.afi_profesiones
                    WHERE descripcion = @Descripcion;";

        private const string SqlProfesionUpdate = @"
                    UPDATE dbo.afi_profesiones
                    SET descripcion = @Descripcion
                    WHERE cod_profesion = @Codigo;";

        private const string SqlProfesionDelete = @"
                    DELETE FROM dbo.afi_profesiones
                    WHERE cod_profesion = @Codigo;";

        private static readonly IReadOnlyDictionary<string, int> ProfesionesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_profesion"] = 1,
            ["descripcion"] = 2
        };

        public FrmAFProfesionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista paginada de profesiones.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de profesiones.</returns>
        public ErrorDto<TablasListaGenericaModel> AF_Profesiones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, ProfesionesSortMap, "cod_profesion");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new TablasListaGenericaModel
            {
                total = connection.QueryFirstOrDefault<int>(SqlProfesionesTotal, spec.Params),
                lista = connection.Query<DropDownListaGenericaModel>(SqlProfesionesLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener profesiones.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }


        /// <summary>
        /// Inserta o actualiza una profesión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Codigo">Código de profesión o Nuevo.</param>
        /// <param name="Descripcion">Descripción de la profesión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Profesiones_Guardar(int CodEmpresa, string Usuario, string Codigo, string Descripcion)
        {
            var descripcionSegura = NormalizarTexto(Descripcion);
            if (string.IsNullOrWhiteSpace(descripcionSegura))
            {
                return DbHelper.ErrorResponse("La descripción de la profesión es requerida.", -2);
            }

            return EsRegistroNuevo(Codigo)
                ? InsertarProfesion(CodEmpresa, Usuario, descripcionSegura)
                : ActualizarProfesion(CodEmpresa, Codigo, descripcionSegura);
        }


        /// <summary>
        /// Elimina una profesión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Codigo">Código de profesión.</param>
        /// <param name="Descripcion">Descripción de la profesión.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_Profesiones_Eliminar(int CodEmpresa, string Usuario, int Codigo, string Descripcion)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlProfesionDelete,
                new { Codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar profesión.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraProfesion(CodEmpresa, Usuario, Descripcion, "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }
        /// <summary>
        /// Inserta una profesión nueva.
        /// </summary>
        private ErrorDto InsertarProfesion(int codEmpresa, string usuario, string descripcion)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                codEmpresa,
                SqlProfesionInsert,
                0,
                new { Descripcion = descripcion });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al insertar profesión.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraProfesion(codEmpresa, usuario, descripcion, "Registra - WEB");
            return new ErrorDto { Code = result.Result, Description = "Ok" };
        }

        /// <summary>
        /// Actualiza una profesión existente.
        /// </summary>
        private ErrorDto ActualizarProfesion(int codEmpresa, string codigo, string descripcion)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                codEmpresa,
                SqlProfesionUpdate,
                new
                {
                    Codigo = NormalizarTexto(codigo),
                    Descripcion = descripcion
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar profesión.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Determina si el registro corresponde a una inserción nueva.
        /// </summary>
        private static bool EsRegistroNuevo(string? codigo)
        {
            return string.Equals(NormalizarTexto(codigo), CodigoNuevo, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registra en bitácora la operación sobre profesiones.
        /// </summary>
        private void RegistrarBitacoraProfesion(int codEmpresa, string usuario, string descripcion, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Profesion : {NormalizarTexto(descripcion)}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        /// <summary>
        /// Crea una lista vacía para resultados paginados.
        /// </summary>
        private static TablasListaGenericaModel CrearListaVacia()
        {
            return new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
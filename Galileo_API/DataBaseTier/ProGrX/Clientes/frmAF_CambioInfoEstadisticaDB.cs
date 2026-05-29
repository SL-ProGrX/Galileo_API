using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfCambioInfoEstadisticaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1; //Modulo de clientes
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlListaSectores = @"
                    SELECT cod_sector AS item,
                           descripcion
                    FROM dbo.afi_sectores;";

        private const string SqlListaProfesiones = @"
                    SELECT cod_profesion AS item,
                           descripcion
                    FROM dbo.afi_profesiones;";

        private const string SqlListaInstitucionesDeductoras = @"
                    SELECT COD_INSTITUCION AS item,
                           RTRIM(DESCRIPCION) + SPACE(10) + '[' + RTRIM(ISNULL(DESC_CORTA, '')) + ']' AS descripcion
                    FROM dbo.INSTITUCIONES
                    WHERE ACTIVA = 1
                      AND DEDUCCION_PLANILLA = 1
                    ORDER BY RTRIM(DESC_CORTA);";

        private const string SqlUpdateSector = @"
                    UPDATE dbo.socios
                    SET cod_sector = @Codigo
                    WHERE cedula = @Cedula;";

        private const string SqlUpdateProfesion = @"
                    UPDATE dbo.socios
                    SET cod_profesion = @Codigo
                    WHERE cedula = @Cedula;";

        private const string SqlUpdateDeductora = @"
                    UPDATE dbo.socios
                    SET cod_Deductora = @Codigo
                    WHERE cedula = @Cedula;";

        private const string SqlUpdateInstitucion = @"
                    UPDATE dbo.socios
                    SET cod_Institucion = @Codigo
                    WHERE cedula = @Cedula;";

        private static readonly IReadOnlyDictionary<string, string> ListasPorTipo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["S"] = SqlListaSectores,
            ["P"] = SqlListaProfesiones,
            ["D"] = SqlListaInstitucionesDeductoras,
            ["I"] = SqlListaInstitucionesDeductoras
        };

        private static readonly IReadOnlyDictionary<string, string> UpdatesPorTipo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["S"] = SqlUpdateSector,
            ["P"] = SqlUpdateProfesion,
            ["D"] = SqlUpdateDeductora,
            ["I"] = SqlUpdateInstitucion
        };

        public FrmAfCambioInfoEstadisticaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de valores disponibles para cambio masivo de información estadística.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="vTipo">Tipo de información: S sectores, P profesiones, D deductoras, I instituciones.</param>
        /// <returns>Lista de valores disponibles para el tipo solicitado.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CambioInfoEstadistica_Listas_Obtener(int CodEmpresa, string vTipo)
        {
            var tipoSeguro = NormalizarTexto(vTipo).ToUpperInvariant();
            if (!ListasPorTipo.TryGetValue(tipoSeguro, out var sql))
            {
                return DbHelper.CreateErrorResponse(
                    "Tipo de información estadística no válido.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }


        /// <summary>
        /// Procesa el cambio masivo de información estadística para una lista de personas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="vTipo">Tipo de información: S sectores, P profesiones, D deductoras, I instituciones.</param>
        /// <param name="vCodigo">Código que se aplicará a las personas seleccionadas.</param>
        /// <param name="cedulas">Lista de cédulas a procesar.</param>
        /// <returns>Resultado del proceso masivo.</returns>
        public ErrorDto AF_CambioInfoEstadistica_Procesar(int CodEmpresa, string usuario, string vTipo, int vCodigo, List<AfCambioInfoEstadisticaDatos> cedulas)
        {
            var tipoSeguro = NormalizarTexto(vTipo).ToUpperInvariant();
            if (!UpdatesPorTipo.TryGetValue(tipoSeguro, out var sqlUpdate))
            {
                return DbHelper.ErrorResponse("Tipo de información estadística no válido.", -2);
            }

            var cedulasSeguras = ObtenerCedulasValidas(cedulas);
            if (cedulasSeguras.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe indicar al menos una cédula válida.", -2);
            }

            var errores = new List<string>();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var cedula in cedulasSeguras)
                {
                    try
                    {
                        connection.Execute(sqlUpdate, new
                        {
                            Codigo = vCodigo,
                            Cedula = cedula
                        });
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error al procesar la cédula {cedula}: {ex.Message}");
                    }
                }

                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al procesar cambio masivo.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraCambioEstadistica(CodEmpresa, usuario, tipoSeguro, vCodigo, cedulasSeguras.Count);

            return errores.Count > 0
                ? DbHelper.ErrorResponse(string.Join(Environment.NewLine, errores), -1)
                : DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene las cédulas válidas del listado recibido.
        /// </summary>
        /// <param name="cedulas">Listado recibido desde el proceso masivo.</param>
        /// <returns>Lista de cédulas normalizadas y sin duplicados.</returns>
        private static List<string> ObtenerCedulasValidas(List<AfCambioInfoEstadisticaDatos>? cedulas)
        {
            return cedulas?
                .Select(item => NormalizarTexto(item.cedula))
                .Where(cedula => !string.IsNullOrWhiteSpace(cedula))
                .Distinct()
                .ToList() ?? new List<string>();
        }


        /// <summary>
        /// Registra en bitácora el cambio masivo de información estadística.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="tipo">Tipo de información procesada.</param>
        /// <param name="codigo">Código aplicado.</param>
        /// <param name="lineas">Cantidad de cédulas procesadas.</param>
        private void RegistrarBitacoraCambioEstadistica(int codEmpresa, string usuario, string tipo, int codigo, int lineas)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Cambio Masivo de {tipo} - Código {codigo}, Listado de Excel: Líneas({lineas})",
                Movimiento = "Aplica - WEB",
                Modulo = vModulo
            });
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
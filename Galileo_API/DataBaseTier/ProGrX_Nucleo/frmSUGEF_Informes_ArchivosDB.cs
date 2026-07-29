using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSugefInformesArchivosDB
    {

        private readonly IConfiguration _config;

        public FrmSugefInformesArchivosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Consulta los cortes disponibles para informes de archivos SUGEF
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SugefInformesArchivosData>> SUGEFInformesArchivos_Cortes_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SugefInformesArchivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SugefInformesArchivosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@" select Corte, Descripcion, Genera_Base, Genera_Fecha, Genera_Usuario, Archivo_Genera, Archivo_Fecha,
                                    Archivo_Usuario, Rango_Inicio, Rango_Corte  
                                        From SUGEF_Facilidades_Crediticias_Cortes order by Corte desc ";

                result.Result = connection.Query<SugefInformesArchivosData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta los datos de informes de archivos SUGEF para un corte específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<List<SugefFacilidadesCrediticiasData>> SUGEFInformesArchivos_Obtener(int CodEmpresa, DateTime Corte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SugefFacilidadesCrediticiasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SugefFacilidadesCrediticiasData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                Corte = Corte.Date;

                var query = $@"select Id, Accion, NumerdoIdentificacion, TipoIdentificacion, NombreCliente, PrimerApellidoCliente, SegundoApellidoCliente, NombreEmpresa
                                     , TipoReporte, TipoOperacion, TipoMovimiento, TipoIngreso, TipoSalida, TipoMonedaMovimiento
                                     , MontoMovimiento, FechaTransaccion, MotivoTransaccion, OrigenRecursos, MotivoCredito
                                     from SUGEF_Facilidades_Crediticias Where Corte = @Corte order by Id";

                result.Result = connection.Query<SugefFacilidadesCrediticiasData>(query, new { Corte }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Generar el proceso de corte para informes de archivos SUGEF
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Corte"></param>
        /// <param name="Descripcion"></param>
        /// <param name="RngInicio"></param>
        /// <param name="RngCorte"></param>
        /// <returns></returns>
        public ErrorDto SUGEFInformesArchivos_Corte_Procesar(int CodEmpresa, string Usuario, DateTime Corte, string Descripcion, DateTime RngInicio, DateTime RngCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"exec spSUGEF_Facilidades_Crediticias_Corte @Corte, @Descripcion,@RngInicio,@RngCorte,@Usuario";
                connection.Execute(query, new
                {
                    Corte,
                    Descripcion,
                    RngInicio,
                    RngCorte,
                    Usuario
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }


        /// <summary>
        /// Generar el archivo para informes de archivos SUGEF
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<ArchivoDescargaDto> SUGEFInformesArchivos_Archivo(int CodEmpresa, string Usuario, DateTime Corte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"exec spSUGEF_Facilidades_Crediticias_Archivo @Corte,@Usuario";
               var registros = connection.Query<SugefFacilidadesXmlRegistro>(
                query,
                new
                {
                      Corte,
                    Usuario = Usuario.Trim()
                },
                commandType: CommandType.Text,
                commandTimeout: 0)
            .Where(xml => !string.IsNullOrWhiteSpace(xml.XML_TEXT))
            .OrderBy(x => x.IdLinea)
            .ToList();


                if (registros.Count == 0)
                {
                    return DbHelper.CreateErrorResponse<ArchivoDescargaDto>(
                        "No se encontraron datos para generar el archivo.",
                        -1 );
                }

                var contenidoXml = CrearContenidoXml(registros);
                var nombreCorte = LimpiarNombreArchivo(Corte.ToString("yyyyMMdd"));

                var archivo = new ArchivoDescargaDto
                {
                    Contenido = Encoding.UTF8.GetBytes(contenidoXml),
                    NombreArchivo = $"Facilidades_Crediticia_{nombreCorte}.xml",
                    ContentType = "application/xml"
                };

                return DbHelper.CreateOkResponse(archivo);
            }
            catch (Exception )
            {
                return DbHelper.CreateErrorResponse<ArchivoDescargaDto>(
                 "Ocurrió un error al generar el archivo de facilidades crediticias.",
                 -1 );
            }
        

        }
        /// <summary>
        ///  Crea el contenido del archivo XML a partir de los registros obtenidos de la base de datos.
        /// </summary>
        /// <param name="lineas"></param>
        /// <returns></returns>
        private static string CrearContenidoXml( IEnumerable<SugefFacilidadesXmlRegistro> lineas)
        {
            var contenido = new StringBuilder();

            foreach (var linea in lineas)
            {
                contenido.AppendLine(linea.XML_TEXT);
            }

            return contenido.ToString();
        }

        /// <summary>
        /// Limpia el nombre del archivo eliminando caracteres inválidos para nombres de archivo.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static string LimpiarNombreArchivo(string valor)
        {
            var caracteresInvalidos = Path.GetInvalidFileNameChars();

            return new string(
                valor.Where(caracter => !caracteresInvalidos.Contains(caracter))
                     .ToArray());
        }
        public sealed class SugefFacilidadesXmlRegistro
        {
            public int IdLinea { get; set; }

            public string XML_TEXT { get; set; } = string.Empty;
        }

    }
}

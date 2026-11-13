using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos;
using System.Data;
using System.IO.Compression;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels; 


namespace Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL
{
    public class CcProcesoMensualArchivosBL
    {

        private readonly IEnumerable<ICcProcesoMensualArchivoGenerator> _generadorArchivos;
        private readonly CcProcesoMensualEnvioDb _db;
        private readonly PortalDB _portalDb;
        private readonly IConfiguration _config;
        private readonly string _rutaBaseArchivos;

        public CcProcesoMensualArchivosBL(IEnumerable<ICcProcesoMensualArchivoGenerator> generadores, IConfiguration config)
        {
            _generadorArchivos = generadores;
            _db = new CcProcesoMensualEnvioDb(config);
            _portalDb = new PortalDB(config);
            _config = config;
            _rutaBaseArchivos = config["ArchivosGenerados:RutaBase"] ?? string.Empty;
        }

        public ErrorDto<CcProcesoMensualGeneraDeduccionesResponse> CcProcesoMensual_GeneraDeducciones_Ejecutar(int codEmpresa, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            var deduccionesResp = _db.CcProcesoMensual_GeneraDeducciones_Ejecutar(codEmpresa, request);

             if(!deduccionesResp.Result)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualGeneraDeduccionesResponse>(
                    deduccionesResp.Description ?? "No se pudo generar las deducciones.",
                    -1,
                    new CcProcesoMensualGeneraDeduccionesResponse { }
                );
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            CcProcesoMensualGeneraArchivoRequest archivoRequest = new()
            {
                CodInstitucion = request.CodInstitucion,
                EmpresaId = codEmpresa,
                NombreInstitucion = request.NombreInstitucion,
                FechaProceso = request.FechaProceso,
                Usuario = request.Usuario
            };

            var archivoGenerado = GenerarArchivo(connection,archivoRequest);

            if (!archivoGenerado.Generado)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualGeneraDeduccionesResponse>(
                    "Las deducciones se generaron, pero no existen registros para construir el archivo de la institución y el período seleccionados.",
                    -1,
                    new CcProcesoMensualGeneraDeduccionesResponse
                    {
                        Generado = false,
                        Archivo = archivoGenerado
                    });
            }

            CompletarArchivoDescarga(archivoGenerado);

            if (archivoGenerado.ArchivoBytes.Length == 0)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualGeneraDeduccionesResponse>(
                    "El archivo de deducciones se generó sin contenido descargable.",
                    -1,
                    new CcProcesoMensualGeneraDeduccionesResponse
                    {
                        Generado = false,
                        Archivo = archivoGenerado
                    });
            }

            return DbHelper.CreateOkResponse(new CcProcesoMensualGeneraDeduccionesResponse
            {
                Generado = true, 
                Archivo = archivoGenerado
            });
        }



        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request )
        {
            MProcesoMensualDb.SbBitacoraPlanilla(connection,
                new CcProcesoMensualBitacoraPlanillaDto
                {
                    Transaccion = "02.1",
                    CodInstitucion = request.CodInstitucion,
                    Proceso = request.FechaProceso,
                    Gestion = "E",
                    Usuario = request.Usuario,                     
                } );

            var globalesResp = new MProGrxMain(_config).sbSifParametrosInicializa(request.EmpresaId, request.Usuario);
            request.NombreEmpresa = globalesResp?.Result is null
                    ? string.Empty
                    : globalesResp.Result.GstrNombreEmpresa;

            var planillaEnvio = ObtenerPlanillaEnvio( connection,  request.CodInstitucion);

            return EjecutarGenerador( connection, request, planillaEnvio);
        }
        private CcProcesoMensualArchivoGeneradoModel EjecutarGenerador(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request, string planillaEnvio)
        {
            var codigo = planillaEnvio.Trim();

            return codigo switch
            {

                "00" => GenerarPorCodigo(connection, request, "00"),

                "01" => GenerarArchivoF01(connection, request),

                "03" => GenerarArchivoF03(connection, request),

                "05" => GenerarF05YOld(connection, request),

                "11" => CrearRespuestaSinGenerar(codigo),

                "25" or "30" => GenerarPorCodigo(connection, request, "25"),

                "32" or "33" => GenerarPorCodigo(connection, request, "32"),

                _ => GenerarPorCodigo(connection, request, codigo)
            };
        }

        private CcProcesoMensualArchivoGeneradoModel GenerarArchivoF01(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var resultado = GenerarPorCodigo(connection, request, "01");
            var archivosGenerados = resultado.ArchivosGenerados
                .Where(ruta => !string.IsNullOrWhiteSpace(ruta))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (archivosGenerados.Count <= 1)
            {
                return resultado;
            }

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = "01",
                NombreArchivo = CrearNombreZipF01(archivosGenerados[0]),
                RutaArchivo = string.Join("; ", archivosGenerados),
                ContentType = "application/zip",
                ArchivoBytes = CrearZip(archivosGenerados),
                ArchivosGenerados = archivosGenerados
            };
        }

        private static string CrearNombreZipF01(string rutaPrimerArchivo)
        {
            var nombreBase = Path.GetFileNameWithoutExtension(rutaPrimerArchivo);
            var indiceCodigoArchivo = nombreBase.LastIndexOf("-ARC", StringComparison.OrdinalIgnoreCase);

            if (indiceCodigoArchivo > 0)
            {
                nombreBase = nombreBase[..indiceCodigoArchivo];
            }

            return $"{nombreBase}-CCSS.zip";
        }

        private CcProcesoMensualArchivoGeneradoModel GenerarPorCodigo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request, string codigoPlanillaEnvio)
        {
            var generador = BuscarGenerador(codigoPlanillaEnvio);

            return generador.GenerarArchivo(connection, request);
        }
        private static string ObtenerPlanillaEnvio(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT ISNULL(planilla_envio, '') AS PlanillaEnvio
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<string>(
                query,
                new { CodInstitucion = codInstitucion }) ?? string.Empty;
        }
        private static CcProcesoMensualArchivoGeneradoModel CrearRespuestaSinGenerar(string codigoPlanillaEnvio)
        {
            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = false,
                CodigoPlanillaEnvio = codigoPlanillaEnvio,
                NombreArchivo = string.Empty,
                RutaArchivo = string.Empty,
                ContentType = string.Empty,
                ArchivoBytes = [],
                ArchivosGenerados = []
            };
        }
        private ICcProcesoMensualArchivoGenerator? BuscarGeneradorOpcional(string codigoPlanillaEnvio)
        {
            return _generadorArchivos.FirstOrDefault(g =>
                g.CodigosPlanillaEnvio.Any(codigo =>
                    string.Equals(
                        codigo.Trim(),
                        codigoPlanillaEnvio.Trim(),
                        StringComparison.OrdinalIgnoreCase)));
        }
        private ICcProcesoMensualArchivoGenerator BuscarGenerador(string codigoPlanillaEnvio)
        {
            var generador = BuscarGeneradorOpcional(codigoPlanillaEnvio);

            return generador is null
                ? throw new InvalidOperationException(
                    $"No existe generador configurado para planilla_envio '{codigoPlanillaEnvio}'.")
                : generador;
        }
        private CcProcesoMensualArchivoGeneradoModel GenerarArchivoF03(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            if (request.EmpresaId is not (1 or 61))
            {
                return GenerarPorCodigo(connection, request, "03_S");
            }

            var archivosGenerados = new List<string>();

            foreach (var unidad in new[] { "01", "02", "03" })
            {
                var requestUnidad = ClonarRequest(request);
                requestUnidad.Unidad = unidad;

                var resultadoUnidad = GenerarPorCodigo(
                    connection,
                    requestUnidad,
                    "03_A");

                archivosGenerados.AddRange(resultadoUnidad.ArchivosGenerados);
            }

            return CombinarResultadosF03(archivosGenerados);
        }

        private CcProcesoMensualArchivoGeneradoModel CombinarResultadosF03(List<string> archivosGenerados)
        {
            var archivosConRegistros = archivosGenerados
                .Where(ArchivoF03TieneRegistros)
                .ToList();

            // Nota de compatibilidad: anteriormente se tomaban NombreArchivo, RutaArchivo y
            // ContentType de la ultima respuesta del ciclo, por lo que siempre se descargaba
            // u03 aunque ese archivo contuviera unicamente el marcador de cierre "!".
            // Se deja documentado para poder identificar y restaurar ese comportamiento si
            // alguna integracion llegara a requerirlo nuevamente.
            if (archivosConRegistros.Count == 0)
            {
                return CrearRespuestaSinGenerar("03");
            }

            if (archivosConRegistros.Count == 1)
            {
                var rutaArchivo = archivosConRegistros[0];

                return new CcProcesoMensualArchivoGeneradoModel
                {
                    Generado = true,
                    CodigoPlanillaEnvio = "03",
                    NombreArchivo = Path.GetFileName(rutaArchivo),
                    RutaArchivo = rutaArchivo,
                    ContentType = "text/plain",
                    ArchivoBytes = [],
                    ArchivosGenerados = archivosConRegistros
                };
            }

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = "03",
                NombreArchivo = CrearNombreZipF03(archivosConRegistros[0]),
                RutaArchivo = string.Join("; ", archivosConRegistros),
                ContentType = "application/zip",
                ArchivoBytes = CrearZip(archivosConRegistros),
                ArchivosGenerados = archivosConRegistros
            };
        }

        private bool ArchivoF03TieneRegistros(string rutaArchivo)
        {
            var rutaCompleta = ObtenerRutaArchivoDescarga(rutaArchivo);

            return File.Exists(rutaCompleta)
                && File.ReadLines(rutaCompleta)
                    .Any(linea =>
                        !string.IsNullOrWhiteSpace(linea)
                        && !string.Equals(linea.Trim(), "!", StringComparison.Ordinal));
        }

        private byte[] CrearZip(IEnumerable<string> archivosGenerados)
        {
            using var contenidoZip = new MemoryStream();

            using (var zip = new ZipArchive(contenidoZip, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var rutaArchivo in archivosGenerados)
                {
                    var rutaCompleta = ObtenerRutaArchivoDescarga(rutaArchivo);
                    zip.CreateEntryFromFile(
                        rutaCompleta,
                        Path.GetFileName(rutaCompleta),
                        CompressionLevel.Optimal);
                }
            }

            return contenidoZip.ToArray();
        }

        private static string CrearNombreZipF03(string rutaPrimerArchivo)
        {
            var nombreBase = Path.GetFileNameWithoutExtension(rutaPrimerArchivo);

            if (nombreBase.Length >= 3
                && nombreBase[^3] == 'u'
                && char.IsDigit(nombreBase[^2])
                && char.IsDigit(nombreBase[^1]))
            {
                nombreBase = nombreBase[..^3];
            }

            return $"{nombreBase}.zip";
        }
        private static CcProcesoMensualGeneraArchivoRequest ClonarRequest(CcProcesoMensualGeneraArchivoRequest request)
        {
            return new CcProcesoMensualGeneraArchivoRequest
            {
                CodInstitucion = request.CodInstitucion,
                FechaProceso = request.FechaProceso,
                EmpresaId = request.EmpresaId,
                Usuario = request.Usuario,
                NombreInstitucion = request.NombreInstitucion,
                NombreEmpresa = request.NombreEmpresa, 
                Unidad = request.Unidad
            };
        }
        private CcProcesoMensualArchivoGeneradoModel GenerarF05YOld( IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var archivosGenerados = new List<string>();

            var resultadoNuevo = GenerarPorCodigo(
                connection,
                request,
                "05");

            archivosGenerados.AddRange(resultadoNuevo.ArchivosGenerados);

            var resultadoOld = GenerarPorCodigo(
                connection,
                request,
                "05_OLD");

            archivosGenerados.AddRange(resultadoOld.ArchivosGenerados);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = archivosGenerados.Count > 0,
                CodigoPlanillaEnvio = "05",
                NombreArchivo = resultadoOld.NombreArchivo,
                RutaArchivo = resultadoOld.RutaArchivo,
                ContentType = resultadoOld.ContentType,
                ArchivoBytes = [],
                ArchivosGenerados = archivosGenerados
            };
        }

        public ErrorDto<CcProcesoMensualArchivoGeneradoModel> GenerarArchivo_Ejecutar(int codEmpresa, CcProcesoMensualGeneraArchivoRequest request)
        {
            using var connection = DbHelper.OpenConnection(  _portalDb, codEmpresa);
             

            connection.Open();

            var archivo = GenerarArchivo(connection, request);

            if (!archivo.Generado)
            {
                return DbHelper.CreateErrorResponse(
                    "No existen deducciones para la institución y el período seleccionados.",
                    -1,
                    archivo);
            }

            CompletarArchivoDescarga(archivo);

            if (archivo.ArchivoBytes.Length == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El archivo se generó sin contenido descargable.",
                    -1,
                    archivo);
            }

            return DbHelper.CreateOkResponse(archivo);
        }

        private void CompletarArchivoDescarga(CcProcesoMensualArchivoGeneradoModel archivo)
        {
            if (!DebeCargarArchivo(archivo))
            {
                return;
            }

            var rutaArchivo = ObtenerRutaArchivoDescarga(archivo.RutaArchivo);

            if (!File.Exists(rutaArchivo))
            {
                return;
            }

            archivo.ArchivoBytes = File.ReadAllBytes(rutaArchivo);

            if (string.IsNullOrWhiteSpace(archivo.NombreArchivo))
            {
                archivo.NombreArchivo = Path.GetFileName(rutaArchivo);
            }
        }

        private static bool DebeCargarArchivo(CcProcesoMensualArchivoGeneradoModel archivo)
        {
            return archivo.Generado
                && archivo.ArchivoBytes.Length == 0
                && !string.IsNullOrWhiteSpace(archivo.RutaArchivo);
        }

        private string ObtenerRutaArchivoDescarga(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(_rutaBaseArchivos))
            {
                throw new InvalidOperationException("La ruta base de archivos generados no está configurada.");
            }

            var rutaBase = Path.GetFullPath(_rutaBaseArchivos);
            var rutaArchivoCompleta = SafePath.RootPath(rutaArchivo, nameof(rutaArchivo));

            ValidarRutaArchivoGenerado(rutaBase, rutaArchivoCompleta);

            return rutaArchivoCompleta;
        }

        private static void ValidarRutaArchivoGenerado(string rutaBase, string rutaArchivo)
        {
            var rutaBaseNormalizada = rutaBase.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? rutaBase
                : rutaBase + Path.DirectorySeparatorChar;

            if (!rutaArchivo.StartsWith(rutaBaseNormalizada, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La ruta del archivo generado está fuera del directorio permitido.");
            }
        }
    }
}

using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels; 


namespace Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL
{
    public class CcProcesoMensualArchivosBL
    {

        private readonly IEnumerable<ICcProcesoMensualArchivoGenerator> _generadorArchivos;
        private readonly CcProcesoMensualEnvioDb _db;
        private readonly PortalDB _portalDb;
    
        public CcProcesoMensualArchivosBL(IEnumerable<ICcProcesoMensualArchivoGenerator> generadores, IConfiguration config)
        {
            _generadorArchivos = generadores;
            _db = new CcProcesoMensualEnvioDb(config);
            _portalDb = new PortalDB(config);
           
        }

        public ErrorDto<CcProcesoMensualGeneraDeduccionesResponse> CcProcesoMensual_GeneraDeducciones_Ejecutar(int codEmpresa, CcProcesoMensualGeneraDeduccionesRequest request)
        {
            var deduccionesResp = _db.CcProcesoMensual_GeneraDeducciones_Ejecutar(codEmpresa, request);

             if(!deduccionesResp.Result)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualGeneraDeduccionesResponse>(
                    "No se pudo generar las deducciones.",
                    -1,
                    new CcProcesoMensualGeneraDeduccionesResponse { }
                );
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            CcProcesoMensualGeneraArchivoRequest archivoRequest = new()
            {
                CodInstitucion = request.CodInstitucion,
                NombreInstitucion= request.NombreInstitucion,
                FechaProceso = request.FechaProceso,
                Usuario = request.Usuario
            };

            var archivoGenerado = GenerarArchivo(connection,archivoRequest);

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
             

            var planillaEnvio = ObtenerPlanillaEnvio( connection,  request.CodInstitucion);

            return EjecutarGenerador( connection, request, planillaEnvio);
        }
        private CcProcesoMensualArchivoGeneradoModel EjecutarGenerador(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request, string planillaEnvio)
        {
            var codigo = planillaEnvio.Trim();

            return codigo switch
            {

                "00" => GenerarPorCodigo(connection, request, "00"),

                "03" => GenerarArchivoF03(connection, request),

                "05" => GenerarF05YOld(connection, request),

                "11" => CrearRespuestaSinGenerar(codigo),

                "25" or "30" => GenerarPorCodigo(connection, request, "25"),

                "32" or "33" => GenerarPorCodigo(connection, request, "32"),

                _ => GenerarPorCodigo(connection, request, codigo)
            };
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
            CcProcesoMensualArchivoGeneradoModel? ultimaRespuesta = null;

            foreach (var unidad in new[] { "01", "02", "03" })
            {
                var requestUnidad = ClonarRequest(request);
                requestUnidad.Unidad = unidad;

                ultimaRespuesta = GenerarPorCodigo(
                    connection,
                    requestUnidad,
                    "03_A");

                archivosGenerados.AddRange(ultimaRespuesta.ArchivosGenerados);
            }
            return CombinarResultados("03", ultimaRespuesta, archivosGenerados);
        }
        private static CcProcesoMensualArchivoGeneradoModel CombinarResultados(string codigo, CcProcesoMensualArchivoGeneradoModel? ultimoResultado, List<string> archivosGenerados)
        {
            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = archivosGenerados.Count > 0,
                CodigoPlanillaEnvio = codigo,
                NombreArchivo = ultimoResultado?.NombreArchivo ?? string.Empty,
                RutaArchivo = ultimoResultado?.RutaArchivo ?? string.Empty,
                ContentType = ultimoResultado?.ContentType ?? string.Empty,
                ArchivoBytes = [],
                ArchivosGenerados = archivosGenerados
            };
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

            return DbHelper.CreateOkResponse(archivo);
        }
    }
}

using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        public ErrorDto<FndDocumentoConsecutivoResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivo_Obtener(
            int CodEmpresa,
            FndDocumentoConsecutivoRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<FndDocumentoConsecutivoResult?>(
                    "Los datos del documento son requeridos.",
                    -2,
                    null);
            }
            request.sysDocVersion ??= ObtenerSysDocVersion(CodEmpresa);

            return ObtenerSysDocVersion(CodEmpresa) == 1
                ? ObtenerConsecutivoFndVersionUno(CodEmpresa, request.Tipo, request.Operadora)
                : DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<FndDocumentoConsecutivoResult>(
                        SpDocsConsecutivo,
                        new { Tipo = NormalizarTipoDocumento(request.Tipo) },
                        commandType: System.Data.CommandType.StoredProcedure));
        }

        public ErrorDto<FndDocumentoConsecutivoAseResult?> Fnd_TrasladoPatrimonio_DocumentoConsecutivoAse_Obtener(
            int CodEmpresa,
            FndDocumentoConsecutivoAseRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<FndDocumentoConsecutivoAseResult?>(
                    "Los datos del documento ASE son requeridos.",
                    -2,
                    null);
            }

            request.sysDocVersion ??= ObtenerSysDocVersion(CodEmpresa);

            return ObtenerSysDocVersion(CodEmpresa) == 1
                ? ObtenerConsecutivoAseVersionUno(CodEmpresa, request.tipo)
                : DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<FndDocumentoConsecutivoAseResult>(
                        SpDocsConsecutivo,
                        new { Tipo = NormalizarTexto(request.tipo) },
                        commandType: System.Data.CommandType.StoredProcedure));
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Documento_Insertar(
            int CodEmpresa,
            FndDocumentoInsertRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos del documento son requeridos.", -2);
            }

            return EjecutarSimple(
                CodEmpresa,
                SqlDocumentoInsert,
                request,
                "Error al insertar documento.");
        }

        public ErrorDto<SimpleSuccessResult> Fnd_Asiento_Insertar(
            int CodEmpresa,
            FndAsientoInsertRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos del asiento son requeridos.", -2);
            }

            return EjecutarSimple(
                CodEmpresa,
                SqlAsientoInsert,
                request,
                "Error al insertar asiento.");
        }

        public ErrorDto<SimpleSuccessResult> SifTransaccion_Insertar(
            int CodEmpresa,
            SifTransaccionInsertRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos de la transacción son requeridos.", -2);
            }

            var oficinaTitular = ObtenerOficinaTitular(CodEmpresa, request.RegistroUsuario);
            if (string.IsNullOrWhiteSpace(oficinaTitular))
            {
                return ErrorSimple("No se encontró la oficina titular del usuario.", -2);
            }

            request.CodOficina = oficinaTitular;

            return EjecutarSimple(
                CodEmpresa,
                SqlSifTransaccionInsert,
                request,
                "Error al insertar transacción SIF.");
        }

        public ErrorDto<SimpleSuccessResult> SifDocsAsiento_Ejecutar(
            int CodEmpresa,
            SifDocsAsientoRequest request)
        {
            if (request is null)
            {
                return ErrorSimple("Los datos del asiento SIF son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    SpDocsAsiento,
                    CrearParametrosDocsAsiento(request),
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return CrearRespuestaSimple(result, "Error al ejecutar asiento SIF.");
        }
    }
}

using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesEmisionDocumentosBL
    {
        private readonly FrmTesEmisionDocumentosDb _db;
        private readonly MTesoreria mTesoreria;

        public FrmTesEmisionDocumentosBL(IConfiguration config)
        {
            _db = new FrmTesEmisionDocumentosDb(config);
            mTesoreria = new MTesoreria(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, "Genera");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_TiposDocs_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, banco, "G");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return _db.TES_EmisionDocumento_Formato_Obtener(CodEmpresa, banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return _db.TES_EmisionDocumento_Plan_Obtener(CodEmpresa, banco);
        }

        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {
            return _db.TES_EmisionDocumento_Buscar(CodEmpresa, tipoDoc, banco, plan);
        }

        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            return _db.TES_EmisionDocumento_Solicitudes_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<TesEmisionDocumentoSolicitudesPaginaResult>
            TES_EmisionDocumento_Solicitudes_Pagina_Obtener(
                int CodEmpresa,
                TesEmisionDocumentoSolicitudesPaginaRequest request)
        {
            return _db.TES_EmisionDocumento_Solicitudes_Pagina_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto<string> TES_EmisionDocumento_TipoDocGestion(int CodEmpresa, int banco, string tipoDoc)
        {
            ErrorDto<string> comprobante = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "Comprobante");

            if (comprobante.Result != null && comprobante.Result.Trim() == "04")
            {
                comprobante.Result = comprobante.Description; // Transferencia
            }
            else
            {
                comprobante.Result = "CK";
            }
            return comprobante;
        }

        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            return _db.TES_EmisionDocumento_ValidaNumDocumento(CodEmpresa, banco, tipoDoc, docInicial, cantidadList);
        }

        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            return _db.TES_EmisionDocumento_RevisaCuentas_SP(CodEmpresa, banco);
        }

        public ErrorDto<List<TesTransaccionDto>> TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(int CodEmpresa, int banco, string tipoDoc)
        {
           return new ErrorDto<List<TesTransaccionDto>>() { Result = new List<TesTransaccionDto>() };
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.TES_EmisionDocumento_CtasPuente_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            return _db.TES_EmisionDocumento_CtaPuente_Aplicar(CodEmpresa, Banco, Usuario, Solicitudes);
        }

        public ErrorDto<object> TES_EmisionDocumento_Generar(int CodEmpresa, string filtros)
        {
            return _db.TES_EmisionDocumento_Generar(CodEmpresa, filtros);
        }

        public Task<ErrorDto<TesEmisionGenerarLoteResult>>
            TES_EmisionDocumentos_Sinpe_GenerarLoteAsync(
            TesEmisionGenerarLoteRequest request)
        {
            return _db.TES_EmisionDocumentos_Sinpe_GenerarLoteAsync(
                request);
        }

        public ErrorDto<long> TES_EmisionDocumento_ConsecutivoIniciar(
            int CodEmpresa, int banco, string tipoDoc, string plan)
        {
            return _db.TES_EmisionDocumento_ConsecutivoIniciar(CodEmpresa, banco, tipoDoc, plan);
        }

        public ErrorDto<long> TES_EmisionDocumento_ConsecutivoRevertir(
            int CodEmpresa, int banco, string tipoDoc, string plan)
        {
            return _db.TES_EmisionDocumento_ConsecutivoRevertir(CodEmpresa, banco, tipoDoc, plan);
        }

        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Iniciar(
            int codEmpresa,
            string propietario,
            TesEmisionDocumentosProcesoIniciarRequest request)
        {
            return _db.TES_EmisionDocumentos_Sinpe_Proceso_Iniciar(
                codEmpresa,
                propietario,
                request);
        }

        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Estado_Obtener(
            int codEmpresa,
            Guid procesoId,
            string propietario)
        {
            return _db.TES_EmisionDocumentos_Sinpe_Proceso_Estado_Obtener(
                codEmpresa,
                procesoId);
        }

        public ErrorDto<TesEmisionDocumentosProcesoResult?>
            TES_EmisionDocumentos_Proceso_Activo_Banco_Obtener(
                int codEmpresa,
                int banco)
        {
            return _db
                .TES_EmisionDocumentos_Sinpe_Proceso_Activo_Banco_Obtener(
                    codEmpresa,
                    banco);
        }

        public ErrorDto<IReadOnlyList<TesEmisionProcesoError>>
            TES_EmisionDocumentos_Proceso_Errores_Obtener(
                int codEmpresa,
                Guid procesoId)
        {
            return _db
                .TES_EmisionDocumentos_Sinpe_Proceso_Errores_Obtener(
                    codEmpresa,
                    procesoId);
        }

        public ErrorDto<TesEmisionDocumentosProcesoManifiestoResult> TES_EmisionDocumentos_Proceso_Resultado_Obtener(
            int codEmpresa,
            Guid procesoId,
            string propietario)
        {
            return _db.TES_EmisionDocumentos_Proceso_Resultado_Obtener(
                codEmpresa,
                procesoId,
                propietario);
        }

        public TesEmisionDocumentosArchivoContexto? TES_EmisionDocumentos_Proceso_Archivo_Obtener(
            int codEmpresa,
            Guid procesoId,
            Guid archivoId,
            string propietario)
        {
            return _db.TES_EmisionDocumentos_Proceso_Archivo_Obtener(
                codEmpresa,
                procesoId,
                archivoId,
                propietario);
        }

        public ErrorDto<int> ValidaUsuarioEspecial(int CodEmpresa, string usuario)
        {
            return _db.ValidaUsuarioEspecial(CodEmpresa, usuario);
        }
    }
}

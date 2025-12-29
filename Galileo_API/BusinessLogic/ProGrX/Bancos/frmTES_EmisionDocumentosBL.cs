using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

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

        public ErrorDto<string> TES_EmisionDocumento_TipoDocGestion(int CodEmpresa, int banco, string tipoDoc)
        {
            ErrorDto<string> comprobante = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "Comprobante");

            if (comprobante.Result != null && comprobante.Result.Trim() == "04")
            {
                comprobante.Result = "TE"; // Transferencia
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
            return new ErrorDto<object>() { Result = new object() };
        }
    }
}

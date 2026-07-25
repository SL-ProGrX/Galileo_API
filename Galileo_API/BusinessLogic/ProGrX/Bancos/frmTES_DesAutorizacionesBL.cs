
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesDesAutorizacionesBL
    {

        private readonly FrmTesDesAutorizacionesDB DesAutorizacionesDB;
        private readonly MTesoreria mTesoreria;

        public FrmTesDesAutorizacionesBL(IConfiguration config)
        {
            DesAutorizacionesDB = new FrmTesDesAutorizacionesDB(config);
            mTesoreria = new MTesoreria(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_DesAutorizaciones_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, "Autoriza");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_DesAutorizaciones_TiposDocs_Obtener(int CodEmpresa, string usuario, int banco, int tipo_autorizacion)
        {
            if (tipo_autorizacion == 0)
            {
                return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, banco, "A");
            }
            else
            {
                return mTesoreria.sbTesTiposDocsCargaCboAccesoFirmas(CodEmpresa, usuario, banco, "A");
            }
        }

        public ErrorDto<TesSolicitudesLista> TES_DesAutorizaciones_Obtener(int CodEmpresa, string filtros)
        {
            return DesAutorizacionesDB.TES_DesAutorizaciones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_DesAutorizaciones_Aplicar(int CodEmpresa, string clave, string usuario, int tipo_autorizacion, List<int> solicitudesLista)
        {
            return DesAutorizacionesDB.TES_DesAutorizaciones_Aplicar(CodEmpresa, clave, usuario, tipo_autorizacion, solicitudesLista);
        }
    }
}
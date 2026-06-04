using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRTraspasoTesoreriaBL
    {
        private readonly FrmCRTraspasoTesoreriaDB _db;

        public FrmCRTraspasoTesoreriaBL(IConfiguration config)
        {
            _db = new FrmCRTraspasoTesoreriaDB(config);
        }

        #region remesas
        #endregion

        #region cargar
        #endregion

        #region trasladar

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return _db.Cr_TraspasoTes_Remesas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _db.Cr_TraspasoTesToken_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _db.Cr_TraspasoTesToken_Nuevo(CodEmpresa, usuario);
        }

        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _db.Cr_TraspasoTesTraslado_Buscar(CodEmpresa, cod_remesa);
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones
        #endregion

        #region cambio
        #endregion

        #region consultas
        #endregion

        #region aux.giro
        #endregion
    }
}

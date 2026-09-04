using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvRepGeneralBL
    {
        private readonly FrmInvRepGeneralDB _db;

        public FrmInvRepGeneralBL(IConfiguration config)
        {
            _db = new FrmInvRepGeneralDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Bodegas_Obtener(int CodEmpresa)
        {
            return _db.INV_RepGeneral_Bodegas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Unidades_Obtener(int CodEmpresa)
        {
            return _db.INV_RepGeneral_Unidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Departamentos_Obtener(int CodEmpresa)
        {
            return _db.INV_RepGeneral_Departamentos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Proveedores_Obtener(int CodEmpresa)
        {
            return _db.INV_RepGeneral_Proveedores_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Lineas_Obtener(int CodEmpresa)
        {
            return _db.INV_RepGeneral_Lineas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CprUensLista>>
            INV_RepGeneral_Uens_Obtener(int CodEmpresa, string usuario)
        {
            return _db.INV_RepGeneral_Uens_Obtener(CodEmpresa, usuario);
        }
    }
}
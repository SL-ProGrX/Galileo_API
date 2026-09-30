using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvUnidadesConvBl
    {
        private readonly FrmInvUnidadesConvDb _db;

        public FrmInvUnidadesConvBl(
            IConfiguration config)
        {
            _db = new FrmInvUnidadesConvDb(
                config);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_UnidadesConv_Unidades_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_UnidadesConv_Unidades_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<UnidadesConvLista>
            INV_UnidadesConv_Lista_Obtener(
                int CodEmpresa,
                string CodUnidad)
        {
            return _db
                .INV_UnidadesConv_Lista_Obtener(
                    CodEmpresa,
                    CodUnidad);
        }

        public ErrorDto INV_UnidadesConv_Guardar(
            int CodEmpresa,
            UnidadMedicionConvData equivalencia)
        {
            return _db.INV_UnidadesConv_Guardar(
                CodEmpresa,
                equivalencia);
        }

        public ErrorDto INV_UnidadesConv_Eliminar(
            int CodEmpresa,
            string CodUnidad,
            string CodUnidadDestino)
        {
            return _db.INV_UnidadesConv_Eliminar(
                CodEmpresa,
                CodUnidad,
                CodUnidadDestino);
        }
    }
}
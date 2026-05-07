using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivDetalleGarantiaBL
    {
        private readonly FrmVivDetalleGarantiaDB DB;

        public FrmVivDetalleGarantiaBL(IConfiguration config)
        {
            DB = new FrmVivDetalleGarantiaDB(config);
        }

        public ErrorDto<VivDetalleGarantiaLista> Viv_DetalleGarantia_Lista_Obtener(int CodEmpresa, int idGarantia, short linea)
        {
            return DB.Viv_DetalleGarantia_Lista_Obtener(CodEmpresa, idGarantia, linea);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Viv_DetalleGarantia_Grados_Dropdown_Obtener(int CodEmpresa, string descGradoHipoteca)
        {
            return DB.Viv_DetalleGarantia_Grados_Dropdown_Obtener(CodEmpresa, descGradoHipoteca);
        }

        public ErrorDto Viv_DetalleGarantia_Guardar(int CodEmpresa, VivDetalleGarantiaGuardarDto data, string usuario)
        {
            return DB.Viv_DetalleGarantia_Guardar(CodEmpresa, data, usuario);
        }

        public ErrorDto Viv_DetalleGarantia_Eliminar(int CodEmpresa, VivDetalleGarantiaEliminarDto data)
        {
            return DB.Viv_DetalleGarantia_Eliminar(CodEmpresa, data);
        }
    }
}
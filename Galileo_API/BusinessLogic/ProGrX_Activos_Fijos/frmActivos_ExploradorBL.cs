using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo_API.DataBaseTier.ProGrX_Activos_Fijos;

namespace Galileo_API.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosExploradorBL
    {
        private readonly FrmActivosExploradorDB DbActivosExplorador;

        public FrmActivosExploradorBL(IConfiguration config)
        {
            DbActivosExplorador = new FrmActivosExploradorDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Departamentos(int codEmpresa)
        {
            return DbActivosExplorador.Departamentos(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Secciones(
            int codEmpresa,
            string codDepartamento)
        {
            return DbActivosExplorador.Secciones(codEmpresa, codDepartamento);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposActivo(int codEmpresa)
        {
            return DbActivosExplorador.TiposActivo(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Justificaciones(int codEmpresa)
        {
            return DbActivosExplorador.Justificaciones(codEmpresa);
        }

        public ErrorDto<List<ActivoExploradorDto>> Listar(int codEmpresa,ActivosExploradorFiltrosDto filtros)
        {
            return DbActivosExplorador.Listar(codEmpresa, filtros);
        }

    }
}

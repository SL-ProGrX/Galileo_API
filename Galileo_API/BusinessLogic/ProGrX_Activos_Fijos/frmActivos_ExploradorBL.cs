using Galileo.Models;
using Galileo.Models.ERROR;
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

        public ErrorDto<List<ActivoExploradorDto>> Listar(int codEmpresa, ActivosExploradorFiltrosDto filtros)
        {
            return DbActivosExplorador.Listar(codEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Ubicaciones(int codEmpresa)
        {
            return DbActivosExplorador.Ubicaciones(codEmpresa);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return DbActivosExplorador.FechaServidor_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Responsables(int codEmpresa)
        {
            return DbActivosExplorador.Responsables(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Proveedores(int codEmpresa)
        {
            return DbActivosExplorador.Proveedores(codEmpresa);
        }

        public ErrorDto<List<PeriodoExploradorDto>> Periodos(int codEmpresa, string estado)
        {
            return DbActivosExplorador.Periodos(codEmpresa, estado);
        }

        public ErrorDto<List<ActivosExploradorAsientoDto>> Asientos(int codEmpresa,DateTime fechaPeriodo)
        {
            return DbActivosExplorador.Asientos(codEmpresa, fechaPeriodo);
        }

        public ErrorDto<List<ActivosExploradorAsientoDetalleDto>> AsientoDetalle(int codEmpresa,string numAsiento,DateTime fechaPeriodo)
        {
            return DbActivosExplorador.AsientoDetalle(codEmpresa,numAsiento,fechaPeriodo
            );
        }

        public ErrorDto<List<ActivosExploradorModificacionDto>> AdicionesRetiros(int codEmpresa,DateTime fechaPeriodo)
        {
            return DbActivosExplorador.AdicionesRetiros(codEmpresa,fechaPeriodo
            );
        }




    }
}

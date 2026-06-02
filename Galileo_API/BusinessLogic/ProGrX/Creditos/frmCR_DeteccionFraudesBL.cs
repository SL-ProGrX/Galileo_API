using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRDeteccionFraudesBL
    {
        private readonly FrmCRDeteccionFraudesDB DB;

        public FrmCRDeteccionFraudesBL(IConfiguration config)
        {
            DB = new FrmCRDeteccionFraudesDB(config);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Operaciones_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesDB.CR_DeteccionFraudes_Operaciones_Dropdown_Obtener(CodEmpresa);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Personas_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesDB.CR_DeteccionFraudes_Personas_Dropdown_Obtener(CodEmpresa);
        }

        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesDB.CR_DeteccionFraudes_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_DeteccionFraudes_Usuarios_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_DeteccionFraudes_Comites_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Recursos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas)
        {
            return DB.CR_DeteccionFraudes_Recursos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Destinos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas)
        {
            return DB.CR_DeteccionFraudes_Destinos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        public ErrorDto<CrDeteccionFraudesLineaDescripcionDto> CR_DeteccionFraudes_Linea_Descripcion_Obtener(int CodEmpresa,string? codigo)
        {
            return DB.CR_DeteccionFraudes_Linea_Descripcion_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Lineas_F4_Obtener(int CodEmpresa,string? filtro)
        {
            return DB.CR_DeteccionFraudes_Lineas_F4_Obtener(CodEmpresa, filtro);
        }
        public ErrorDto CR_DeteccionFraudes_PrepararReporte(int CodEmpresa,CrDeteccionFraudesReporteRequest request)
        {
            return DB.CR_DeteccionFraudes_PrepararReporte(CodEmpresa,request);
        }
    }
}
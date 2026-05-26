using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComitesAutorizadoresBL
    {
        private readonly FrmCrComitesAutorizadoresDB DB;

        public FrmCrComitesAutorizadoresBL(IConfiguration config)
        {
            DB = new FrmCrComitesAutorizadoresDB(config);
        }

        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_Puestos_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_Puestos_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_Puestos_Guardar(int CodEmpresa, CrComitesPuestoDto data, string usuario)
        {
            return DB.CR_Puestos_Guardar(CodEmpresa, data, usuario);
        }

        public ErrorDto CR_Puestos_Eliminar(int CodEmpresa, string id_puesto, string usuario)
        {
            return DB.CR_Puestos_Eliminar(CodEmpresa, id_puesto, usuario);
        }

        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_Personas_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_Personas_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_Personas_Guardar(int CodEmpresa, CrComitesPersonaDto data, string usuario)
        {
            return DB.CR_Personas_Guardar(CodEmpresa, data, usuario);
        }

        public ErrorDto CR_Personas_Eliminar(int CodEmpresa, string cedula, string usuario)
        {
            return DB.CR_Personas_Eliminar(CodEmpresa, cedula, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Puestos_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_Puestos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_Comites_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Miembros_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return DB.CR_Asignacion_Miembros_Lista_Obtener(CodEmpresa, id_comite);
        }

        public ErrorDto CR_Asignacion_Miembros_Asignar(int CodEmpresa, CrComitesAsignacionRequest request)
        {
            return DB.CR_Asignacion_Miembros_Asignar(CodEmpresa, request);
        }

        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Autorizadores_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return DB.CR_Asignacion_Autorizadores_Lista_Obtener(CodEmpresa, id_comite);
        }

        public ErrorDto CR_Asignacion_Autorizadores_Asignar(int CodEmpresa, CrComitesAsignacionRequest request)
        {
            return DB.CR_Asignacion_Autorizadores_Asignar(CodEmpresa, request);
        }
    }
}
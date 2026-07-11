using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfReportesBl
    {
        private readonly FrmAfReportesDb DbfrmAF_Reportes;

        public FrmAfReportesBl(IConfiguration config)
        {
            DbfrmAF_Reportes = new FrmAfReportesDb(config);
        }


        public ErrorDto<AfReportesCombosDto> AF_Reportes_Combos_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Reportes_Combos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Provincias_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string provincia)
        {
            return DbfrmAF_Reportes.AF_Cantones_Obtener(CodEmpresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return DbfrmAF_Reportes.AF_Distritos_Obtener(CodEmpresa, provincia, canton);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_UTrabajo_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_UTrabajo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_UProgramatica_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_UProgramatica_Obtener(CodEmpresa);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.FechaServidor_Obtener(CodEmpresa);
        }
        public ErrorDto<List<AfGrupoConfiguracionDto>> AF_Configuracion_Grupos_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Configuracion_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfGrupoMiembroDto>> AF_Configuracion_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            return DbfrmAF_Reportes.AF_Configuracion_Miembros_Obtener(CodEmpresa, CodGrupo);
        }

        public ErrorDto<List<AfReporteDto>> AF_Configuracion_Informes_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Configuracion_Informes_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfSeguridadGrupoDto>> AF_Seguridad_Grupos_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Seguridad_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfSeguridadMiembroDto>> AF_Seguridad_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            return DbfrmAF_Reportes.AF_Seguridad_Miembros_Obtener(CodEmpresa, CodGrupo);
        }

        public ErrorDto<List<AfSeguridadReporteDto>> AF_Seguridad_Reportes_Obtener(int CodEmpresa, string CodGrupo)
        {
            return DbfrmAF_Reportes.AF_Seguridad_Reportes_Obtener(CodEmpresa, CodGrupo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Grupos_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Miembros_Grupos_Obtener(int CodEmpresa)

        {
            return DbfrmAF_Reportes.AF_Miembros_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto AF_Grupos_Guardar(int CodEmpresa, AfGrupoConfiguracionDto grupo)
        {
            return DbfrmAF_Reportes.AF_Grupos_Guardar(CodEmpresa, grupo);
        }

        public ErrorDto AF_Grupos_Eliminar(int CodEmpresa, string cod_grupo)
        {
            return DbfrmAF_Reportes.AF_Grupos_Eliminar(CodEmpresa, cod_grupo);
        }

        public ErrorDto AF_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfGrupoMiembroDto miembro)
        {
            return DbfrmAF_Reportes.AF_Miembros_Guardar(CodEmpresa, cod_grupo, miembro);
        }
        public ErrorDto AF_Reportes_Guardar(int CodEmpresa, AfReporteDto reporte)
        {
            return DbfrmAF_Reportes.AF_Reportes_Guardar(CodEmpresa, reporte);
        }

        public ErrorDto AF_Reportes_Grupo_Guardar(int CodEmpresa, AfSeguridadGrupoDto grupo)
        {
            return DbfrmAF_Reportes.AF_Reportes_Grupo_Guardar(CodEmpresa, grupo);
        }

        public ErrorDto AF_Reportes_Grupo_Eliminar(int CodEmpresa, int codgrupo)
        {
            return DbfrmAF_Reportes.AF_Reportes_Grupo_Eliminar(CodEmpresa, codgrupo);
        }

        public ErrorDto AF_Reportes_Grupo_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfSeguridadMiembroDto miembroseguridad)
        {
            return DbfrmAF_Reportes.AF_Reportes_Grupo_Miembros_Guardar(CodEmpresa, cod_grupo, miembroseguridad);
        }

        public ErrorDto AF_Reportes_Seguridad_Guardar(int CodEmpresa, string id_rep, string cod_grupo)
        {
            return DbfrmAF_Reportes.AF_Reportes_Seguridad_Guardar(CodEmpresa, id_rep, cod_grupo);
        }
    }
}

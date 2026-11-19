using System.Security.Cryptography;
using System.Text;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfReportesBl
    {
        /// <summary>Clave de configuración (appsettings / user-secrets) con la clave de edición de informes.</summary>
        private const string ClaveEdicionConfigKey = "ProGrX:ClaveEdicionInformesAF";

        private readonly FrmAfReportesDb DbfrmAF_Reportes;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;
        private readonly IConfiguration _config;

        public FrmAfReportesBl(IConfiguration config)
        {
            _config = config;
            DbfrmAF_Reportes = new FrmAfReportesDb(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
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

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Departamentos_Obtener(int CodEmpresa)
        {
            return DbfrmAF_Reportes.AF_Departamentos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Secciones_Obtener(int CodEmpresa, string Departamento)
        {
            return DbfrmAF_Reportes.AF_Secciones_Obtener(CodEmpresa, Departamento);
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
        public ErrorDto AF_Reportes_Guardar(int CodEmpresa, AfReporteDto reporte, string? claveEdicion)
        {
            var errorClave = ValidarClaveEdicion(claveEdicion);
            if (errorClave != null)
            {
                return errorClave;
            }

            return DbfrmAF_Reportes.AF_Reportes_Guardar(CodEmpresa, reporte);
        }

        /// <summary>
        /// Clave de edición de informes (VB6: "Clave de Edición (Admin)" en frmAF_Reportes).
        /// La clave vive en configuración del servidor, no en el código.
        /// </summary>
        private ErrorDto? ValidarClaveEdicion(string? claveIngresada)
        {
            var claveEsperada = _config[ClaveEdicionConfigKey];
            if (string.IsNullOrWhiteSpace(claveEsperada))
            {
                return DbHelper.ErrorResponse("La clave de edición no está configurada en el servidor.", -1);
            }

            if (string.IsNullOrEmpty(claveIngresada))
            {
                return DbHelper.ErrorResponse("Debe ingresar la clave de edición.", -1);
            }

            var esperadaBytes = Encoding.UTF8.GetBytes(claveEsperada);
            var ingresadaBytes = Encoding.UTF8.GetBytes(claveIngresada);
            var coincide = esperadaBytes.Length == ingresadaBytes.Length
                && CryptographicOperations.FixedTimeEquals(esperadaBytes, ingresadaBytes);

            return coincide ? null : DbHelper.ErrorResponse("Clave de edición incorrecta.", -1);
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

        public ErrorDto<List<int>> AF_Reportes_Acceso_Obtener(int CodEmpresa, string usuario)
        {
            return DbfrmAF_Reportes.AF_Reportes_Acceso_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaGeneros(int CodEmpresa)
        {
            return _AuxiliarDB.CargaGeneros(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CargaPaises(int CodEmpresa)
        {
            return _AuxiliarDB.CargaPaises(CodEmpresa);
        }
    }
}

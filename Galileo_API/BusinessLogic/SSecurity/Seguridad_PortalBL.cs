using Galileo.DataBaseTier;
using Galileo.Models;

namespace Galileo.BusinessLogic
{
    public class SeguridadPortalBl
    {
        readonly SeguridadPortalDb SecurityDb;
        private readonly IConfiguration _config;

        public SeguridadPortalBl(IConfiguration config)
        {
            _config = config;
            SecurityDb = new SeguridadPortalDb(config);
        }

        public AdminAccessDto sbAdmin_Rols_Load(string pUsuario, int EmpresaId)
        {
            AdminAccessDto accessPortal = new AdminAccessDto();

            accessPortal.Admin_Portal = false;
            accessPortal.Rol_DirGlobal = false;
            accessPortal.Rol_AdminView = false;
            accessPortal.Rol_LocalUsers = false;
            accessPortal.Rol_Permisos = false;
            accessPortal.Rol_ResetKeys = false;

            if (SecurityDb.Sys_Portal_Admin_Valid(pUsuario))
            {
                accessPortal.Admin_Portal = true;
                accessPortal.Rol_DirGlobal = true;
                accessPortal.Rol_AdminView = true;
                accessPortal.Rol_LocalUsers = true;
                accessPortal.Rol_Permisos = true;
                accessPortal.Rol_ResetKeys = true;

                accessPortal.ResultMsg = "OK";
                return accessPortal;
            }

            // Mantiene el orden de mPGX_Portal: solo una cuenta habilitada para
            // administración puede consultar los roles de la empresa.
            if (SecurityDb.UsuarioObtenerKeyAdmin(pUsuario) <= 0)
            {
                accessPortal.ResultMsg = "Este usuario no tiene cuenta de administración para el portal!";
                return accessPortal;
            }

            UsAdminClientesDto usAdminClientesDto = SecurityDb.US_ADMIN_CLIENTES_Obtener(pUsuario, EmpresaId);
            if (usAdminClientesDto != null)
            {

                // Los permisos del administrador del portal son globales y no deben
                // ser rebajados por la fila de permisos de una empresa específica.
                accessPortal.Rol_DirGlobal |= usAdminClientesDto.R_GLOBAL_DIR_SEARCH == 1;
                accessPortal.Rol_AdminView |= usAdminClientesDto.R_ADMIN_REVIEW == 1;
                accessPortal.Rol_LocalUsers |= usAdminClientesDto.R_LOCAL_USERS == 1;
                accessPortal.Rol_Permisos |= usAdminClientesDto.R_LOCAL_GRANTS == 1;
                accessPortal.Rol_ResetKeys |= usAdminClientesDto.R_LOCAL_KEY_RESET == 1;

            }

            var tienePermisoEmpresa = usAdminClientesDto != null &&
                (usAdminClientesDto.R_GLOBAL_DIR_SEARCH == 1 ||
                 usAdminClientesDto.R_ADMIN_REVIEW == 1 ||
                 usAdminClientesDto.R_LOCAL_USERS == 1 ||
                 usAdminClientesDto.R_LOCAL_GRANTS == 1 ||
                 usAdminClientesDto.R_LOCAL_KEY_RESET == 1);

            accessPortal.ResultMsg = tienePermisoEmpresa
                ? "OK"
                : EmpresaId > 0
                    ? "Este usuario no tiene permisos administrativos para la empresa seleccionada."
                    : "Este usuario tiene acceso al portal, pero no cuenta con permisos administrativos asignados a ninguna empresa.";

            return accessPortal;

        }//end sbAdmin_Rols_Load

        /// <summary>
        /// Determina si el usuario tiene permisos efectivos para ingresar al portal SSecurity
        /// antes de que la autenticación cree una sesión persistente. Los roles específicos
        /// por empresa se validan únicamente en las empresas disponibles para ese usuario.
        /// </summary>
        public AdminAccessDto AccesoEfectivoPortalObtener(string pUsuario)
        {
            var accesoGeneral = sbAdmin_Rols_Load(pUsuario, 0);
            if (TienePermisoHabilitante(accesoGeneral))
            {
                return accesoGeneral;
            }

            if (SecurityDb.UsuarioObtenerKeyAdmin(pUsuario) <= 0)
            {
                return accesoGeneral;
            }

            var empresas = new LogonDB(_config).ClientesObtener(pUsuario);
            if (empresas.Code is < 0 || empresas.Result is null)
            {
                throw new InvalidOperationException(
                    empresas.Description ?? "No fue posible consultar las empresas disponibles para validar el acceso a SSecurity.");
            }

            var empresaIds = new HashSet<int> { 0 };
            foreach (var empresa in empresas.Result)
            {
                var codigo = !string.IsNullOrWhiteSpace(empresa.Cod_Empresa)
                    ? empresa.Cod_Empresa
                    : empresa.CodEmpresa;
                if (int.TryParse(codigo, out var empresaId) && empresaId > 0)
                {
                    empresaIds.Add(empresaId);
                }
            }

            foreach (var empresaId in empresaIds)
            {
                if (empresaId == 0)
                {
                    continue;
                }

                var accesoEmpresa = sbAdmin_Rols_Load(pUsuario, empresaId);
                if (TienePermisoHabilitante(accesoEmpresa))
                {
                    return accesoEmpresa;
                }
            }

            return new AdminAccessDto
            {
                ResultMsg = "Este usuario no tiene permisos administrativos para ingresar al portal!",
            };
        }

        private static bool TienePermisoHabilitante(AdminAccessDto permisos)
        {
            return string.Equals(permisos.ResultMsg, "OK", StringComparison.Ordinal) &&
                (permisos.Admin_Portal ||
                 permisos.Rol_DirGlobal ||
                 permisos.Rol_AdminView ||
                 permisos.Rol_LocalUsers ||
                 permisos.Rol_Permisos ||
                 permisos.Rol_ResetKeys);
        }

        public SbgSegInicializaResultDto sbgSEGInicializa(string pUsuario, string AppName, string AppVersion) //Falta AppStatusObtener de Seguridad_PortalDB 
        {

            SbgSegInicializaResultDto Result = new SbgSegInicializaResultDto();

            UsuarioBloqueoDto UsuarioBloqueo = SecurityDb.UsuarioBloqueoObtener(pUsuario);
            UsuarioCondicionDto UsuarioCondicion = SecurityDb.UsuarioCondicionObtener(pUsuario);
            UsuarioVencimientoDto UsuarioVencimiento = SecurityDb.UsuarioVencimientoObtener(pUsuario);
            if (UsuarioBloqueo != null && UsuarioBloqueo.Bloqueo == 1)
            {

                Result.BloqueoMsg = "Su contrase�a se encuentra bloqueada, espere para desbloqueo Autom�tico, o Comuniquese con su Administrador de Sistemas para resolver su situaci�n.";

            }

            if (UsuarioCondicion != null && UsuarioCondicion.KEY_RENEW_SESION == 1)
            {
                Result.CondicionMsg = "El Usuario no renovo su contrase�a...!";
            }

            if (UsuarioVencimiento != null && UsuarioVencimiento.Vencida == 1)
            {
                Result.Vencimiento_VencidaMsg = "Su contrase�a ya se encuentra vencida, procesa a cambiarla.!";
            }

            if (UsuarioVencimiento != null && UsuarioVencimiento.Renovacion == 1)
            {
                string numDias = "";
                if (UsuarioVencimiento.Dias == 0)
                {
                    numDias = "hasta Hoy";
                }
                else
                {
                    numDias = UsuarioVencimiento.Dias + " dias(s)";
                }

                Result.Vencimiento_RenovacionMsg = "Su contrase�a se encuentra pr�xima a vencer, por favor realizar el cambio de clave, antes de que caduque y su login se bloquee. Tiene " + numDias + " de tiempo para realizar el cambio";

            }

            return Result;


        }

        public void sbWebApps_Sincroniza(string pServer, int pPaso, int? Empresa, string? Cedula) //Falta sbWebApps_Sincroniza_Paso2 de Seguridad_PortalDB 
        {
            if (pPaso == 1)
            {
                SecurityDb.sbWebApps_Sincroniza_Paso1y3(pPaso, Empresa, Cedula);
            }

            if (pPaso == 3)
            {
                SecurityDb.sbWebApps_Sincroniza_Paso1y3(pPaso, Empresa, Cedula);
            }
        }

        public void spCore_Usuario_Sincroniza(int pCliente, string pUsuario, string pNombre, string pEstado)// Falta spCore_Usuario_Sincroniza de Seguridad_PortalDB
        {
            SecurityDb.SeleccionarPgxClientePorCodEmpresa(pCliente);
        }

        public string sbSIFMenuOptionClick(int pNodo, int Cliente, string Usuario)
        {
            string result = "";
            UsMenuDto menu = SecurityDb.ObtenerMenuPorNodoYUsuario(pNodo, Usuario);
            if (menu != null)
            {
                if (menu.Acceso == 0)
                {
                    result = "";
                }
                else if (menu.TIPO == "A") // Solo Accesos Directos
                {
                    switch (menu.MODO)
                    {
                        case "F": // Vía Formulario
                        case "C": // Vía Clase
                            SecurityDb.ActualizarEstadisticasFavoritos(pNodo, Cliente, Usuario);
                            break;
                    }
                }
            }
            return result;
        }
    }
}

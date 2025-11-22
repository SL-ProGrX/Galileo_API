using Galileo.DataBaseTier;
using Galileo.Models;

namespace Galileo.BusinessLogic
{
    public class SeguridadPortalBl
    {
        readonly SeguridadPortalDb SecurityDb;

        public SeguridadPortalBl(IConfiguration config)
        {
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
            }

            if (SecurityDb.UsuarioObtenerKeyAdmin(pUsuario) == 0)
            {
                accessPortal.ResultMsg = "Este usuario no tiene cuenta de administración para el portal!";
            }
            else
            {
                accessPortal.ResultMsg = "OK";
            }

            UsAdminClientesDto usAdminClientesDto = SecurityDb.US_ADMIN_CLIENTES_Obtener(pUsuario, EmpresaId);
            if (usAdminClientesDto != null)
            {

                accessPortal.Rol_DirGlobal = usAdminClientesDto.R_GLOBAL_DIR_SEARCH == 1;
                accessPortal.Rol_AdminView = usAdminClientesDto.R_ADMIN_REVIEW == 1;
                accessPortal.Rol_LocalUsers = usAdminClientesDto.R_LOCAL_USERS == 1;
                accessPortal.Rol_Permisos = usAdminClientesDto.R_LOCAL_GRANTS == 1;
                accessPortal.Rol_ResetKeys = usAdminClientesDto.R_LOCAL_KEY_RESET == 1;

            }

            return accessPortal;

        }//end sbAdmin_Rols_Load

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
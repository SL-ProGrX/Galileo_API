using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsDerechosBl
    {
        private readonly IConfiguration _config;
        readonly FrmUsDerechosDb DerechosDB;

        public FrmUsDerechosBl(IConfiguration config)
        {
            _config = config;
            DerechosDB = new FrmUsDerechosDb(_config);
        }

        public ErrorDto<List<UsDerechosNewDto>> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            return DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);

        }//end ObtenerUsDerechosNewDTOs
        
        public ErrorDto<List<UsModuloDto>> ObtenerArbolDerechosNew(string Rol, string Estado)
        {
            try
            {
                var listaModulos = DerechosDB.ObtenerUsModulos();
                var listaFormularios = DerechosDB.ObtenerUsFormularios();
                var listaOpciones = DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);

                if (listaModulos.Code != 0 || listaFormularios.Code != 0 || listaOpciones.Code != 0)
                {
                    var descripcion = ObtenerDescripcionError(
                        listaModulos.Code,
                        listaModulos.Description,
                        listaFormularios.Code,
                        listaFormularios.Description,
                        listaOpciones.Description);

                    return new ErrorDto<List<UsModuloDto>>
                    {
                        Code = -1,
                        Description = descripcion,
                        Result = []
                    };
                }

                foreach (var modulo in listaModulos.Result ?? [])
                {
                    var formulariosDelModulo = (listaFormularios.Result ?? []).Where(x => x.MODULO == modulo.MODULO).ToList();
                    modulo.HijoFormularios = formulariosDelModulo;

                    foreach (var formulario in modulo.HijoFormularios)
                    {
                        var opcionesDelFormulario = (listaOpciones.Result ?? []).Where(x => x.FORMULARIO == formulario.FORMULARIO && x.MODULO == formulario.MODULO).ToList();
                        formulario.Opciones = opcionesDelFormulario;
                    }
                }

                return new ErrorDto<List<UsModuloDto>> { Code = 0, Description = "Ok", Result = listaModulos.Result ?? [] };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<UsModuloDto>> { Code = -1, Description = ex.Message, Result = [] };
            }

        }//end ObtenerArbolDerechosNew
        
        public ErrorDto<List<PrimeTreeDto>> ObtenerArbolDerechosNewPrime(string Rol, string Estado)
        {
            try
            {
                var listaModulos = DerechosDB.ObtenerUsModulos();
                var listaFormularios = DerechosDB.ObtenerUsFormularios();
                var listaOpciones = DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);

                if (listaModulos.Code != 0 || listaFormularios.Code != 0 || listaOpciones.Code != 0)
                {
                    return new ErrorDto<List<PrimeTreeDto>>
                    {
                        Code = -1,
                        Description = ObtenerDescripcionError(
                            listaModulos.Code,
                            listaModulos.Description,
                            listaFormularios.Code,
                            listaFormularios.Description,
                            listaOpciones.Description),
                        Result = []
                    };
                }

                var arbol = ConstruirArbolPrime(
                    listaModulos.Result ?? [],
                    listaFormularios.Result ?? [],
                    listaOpciones.Result ?? []);

                return new ErrorDto<List<PrimeTreeDto>> { Code = 0, Description = "Ok", Result = arbol };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<PrimeTreeDto>> { Code = -1, Description = ex.Message, Result = [] };
            }
        }

        public ErrorDto<List<UsRolDto>> ObtenerUsRoles()
        {
            return DerechosDB.ObtenerUsRoles();
        }//end ObtenerUsRoles

        public ErrorDto CrearUsDerechosNewDTO(List<CrearUsDerechosNewDto> info)
        {
            int contador = 0;
            ErrorDto resultado = new ErrorDto();
            resultado.Code = 0;
            try
            {
                foreach (var item in info)
                {
                    var response = DerechosDB.CrearUsDerechosNewDTO(item);
                    resultado.Code = response.Code;
                    resultado.Description = response.Description;
                    if (response.Code != 0)
                    {
                        if (resultado.Code == 2)
                        {
                            resultado.Description = "El registro ya existe en otro estado";
                        }
                        contador++;
                    }
                }

                if (contador > 0)
                {
                    resultado.Code = -1;
                    resultado.Description += " - " + contador + " errores de " + info.Count.ToString() + " durante el guardado";
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;

        }//end CrearUsDerechosNewDTO

        public ErrorDto EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            ErrorDto resultado = new ErrorDto();
            resultado.Code = 0;
            try
            {
                var response = DerechosDB.EliminarUsDerechosNewDTO(COD_OPCION, ESTADO, COD_ROL);
                resultado.Code = response.Code;
                resultado.Description = response.Description;
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;

        }//end EliminarUsDerechosNewDTO

        public ErrorDto EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            ErrorDto resultado = new ErrorDto();
            resultado.Code = 0;
            try
            {
                var response = DerechosDB.EditarUsDerechosNew(COD_OPCION, ESTADO, COD_ROL, NUEVO_ESTADO);
                resultado.Code = response.Code;
                resultado.Description = response.Description;
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;
        }

        public ErrorDto RegistrarBitacora(SegLogInsertarDto request)
        {
            return new MSecurityMainDb(_config).SbSEGCuentaLog(request);
        }

        private static string? ObtenerDescripcionError(
            int? modulosCode,
            string? modulosDescription,
            int? formulariosCode,
            string? formulariosDescription,
            string? opcionesDescription)
        {
            if (modulosCode != 0)
            {
                return modulosDescription;
            }

            if (formulariosCode != 0)
            {
                return formulariosDescription;
            }

            return opcionesDescription;
        }

        private static List<PrimeTreeDto> ConstruirArbolPrime(
            List<UsModuloDto> modulos,
            List<UsFormularioDto> formularios,
            List<UsDerechosNewDto> opciones)
        {
            var arbol = new List<PrimeTreeDto>();

            foreach (var modulo in modulos)
            {
                var nodoModulo = CrearNodoModulo(modulo);
                var formulariosDelModulo = formularios.Where(x => x.MODULO == modulo.MODULO);

                foreach (var formulario in formulariosDelModulo)
                {
                    nodoModulo.Children!.Add(CrearNodoFormulario(formulario, opciones));
                }

                arbol.Add(nodoModulo);
            }

            return arbol;
        }

        private static PrimeTreeDto CrearNodoModulo(UsModuloDto modulo)
        {
            return new PrimeTreeDto
            {
                Expanded = true,
                Key = modulo.MODULO.ToString(),
                Label = modulo.DESCRIPCION,
                Selectable = true,
                //Icon = "fa-regular fa-window-maximize",
                ExpandedIcon = "fa-regular fa-window-restore",
                CollapsedIcon = "fa-regular fa-window-maximize",
                Style = "font-weight: bold;",
                Children = [],
                Data = modulo,
                leaf = false
            };
        }

        private static PrimeTreeDto CrearNodoFormulario(
            UsFormularioDto formulario,
            List<UsDerechosNewDto> opciones)
        {
            var nodoFormulario = new PrimeTreeDto
            {
                Children = [],
                Expanded = true,
                Key = formulario.FORMULARIO + " del " + formulario.MODULO.ToString(),
                Label = formulario.DESCRIPCION,
                Selectable = true,
                ExpandedIcon = "pi pi-folder-open",
                CollapsedIcon = "pi pi-folder",
                leaf = false,
                Style = "font-weight: normal;",
                Data = formulario
            };

            var opcionesDelFormulario = opciones.Where(x =>
                x.FORMULARIO != null &&
                formulario.FORMULARIO != null &&
                x.FORMULARIO.Trim() == formulario.FORMULARIO.Trim() &&
                x.MODULO == formulario.MODULO);

            foreach (var opcion in opcionesDelFormulario)
            {
                nodoFormulario.Children.Add(CrearNodoOpcion(opcion));
            }

            return nodoFormulario;
        }

        private static PrimeTreeDto CrearNodoOpcion(UsDerechosNewDto opcion)
        {
            return new PrimeTreeDto
            {
                Children = [],
                Expanded = false,
                Key = opcion.COD_OPCION.ToString(),
                Label = opcion.OPCION_DESCRIPCION.ToString(),
                Selectable = true,
                ExpandedIcon = "fa-solid fa-pager",
                CollapsedIcon = "fa-solid fa-pager",
                leaf = true,
                Style = ObtenerColorPermiso(opcion.PermisoEstado),
                Data = opcion
            };
        }

        private static string ObtenerColorPermiso(string permisoEstado)
        {
            if (permisoEstado == "A")
            {
                return "color:#3FB652; font-weight: bold;";
            }

            if (permisoEstado == "R")
            {
                return "color:#FF5B5B; font-weight: bold;";
            }

            return string.Empty;
        }
    }
}

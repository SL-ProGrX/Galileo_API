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

        public List<UsDerechosNewDto> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            List<UsDerechosNewDto> resultado = new List<UsDerechosNewDto>();
            try
            {
                var lista = DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);

                foreach (var item in lista)
                {
                    resultado.Add(new UsDerechosNewDto
                    {
                        COD_OPCION = item.COD_OPCION,
                        FORMULARIO = item.FORMULARIO,
                        MODULO = item.MODULO,
                        OPCION = item.OPCION,
                        OPCION_DESCRIPCION = item.OPCION_DESCRIPCION,
                        REGISTRO_FECHA = item.REGISTRO_FECHA,
                        REGISTRO_USUARIO = item.REGISTRO_USUARIO,
                        PermisoEstado = item.PermisoEstado,

                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }//end ObtenerUsDerechosNewDTOs
        
        public List<UsModuloDto> ObtenerArbolDerechosNew(string Rol, string Estado)
        {
            List<UsModuloDto> Arbol = new();

            try
            {
                var listaModulos = new FrmUsMenusDb(_config).ObtenerUsModulos();
                var listaFormularios = new FrmUsMenusDb(_config).ObtenerUsFormularios();
                var listaOpciones = DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);//obtener opciones

                foreach (var modulo in listaModulos)
                {
                    var formulariosDelModulo = listaFormularios.Where(x => x.MODULO == modulo.MODULO).ToList();
                    modulo.HijoFormularios = formulariosDelModulo;

                    foreach (var formulario in modulo.HijoFormularios)
                    {
                        var opcionesDelFormulario = listaOpciones.Where(x => x.FORMULARIO == formulario.FORMULARIO && x.MODULO == formulario.MODULO).ToList();
                        formulario.Opciones = opcionesDelFormulario;
                    }
                }

                Arbol = listaModulos;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Arbol;

        }//end ObtenerArbolDerechosNew
        
        public List<PrimeTreeDto> ObtenerArbolDerechosNewPrime(string Rol, string Estado)
        {
            List<PrimeTreeDto> Arbol = new();

            try
            {
                List<UsModuloDto> listaModulos = new FrmUsMenusDb(_config).ObtenerUsModulos();
                List<UsFormularioDto> listaFormularios = new FrmUsMenusDb(_config).ObtenerUsFormularios();
                List<UsDerechosNewDto> listaOpciones = DerechosDB.ObtenerUsDerechosNewDTOs(Rol, Estado);//obtener opciones

                foreach (var modulo in listaModulos)
                {
                    var mod = new PrimeTreeDto
                    {
                        Expanded = true,
                        Key = modulo.MODULO.ToString(),
                        Label = modulo.DESCRIPCION,
                        Selectable = true,
                        //Icon = "fa-regular fa-window-maximize",
                        ExpandedIcon = "fa-regular fa-window-restore",
                        CollapsedIcon = "fa-regular fa-window-maximize",
                        Style = "font-weight: bold;",
                        Children = new List<PrimeTreeDto>(),
                        Data = modulo,
                        leaf = false

                    };
                    var formulariosDelModulo = listaFormularios.Where(x => x.MODULO == modulo.MODULO).ToList();

                    foreach (var frm in formulariosDelModulo)
                    {
                        var frmC = new PrimeTreeDto
                        {
                            Children = [],
                            Expanded = true,
                            Key = frm.FORMULARIO + " del " + frm.MODULO.ToString(),
                            Label = frm.DESCRIPCION,
                            Selectable = true,
                            ExpandedIcon = "pi pi-folder-open",
                            CollapsedIcon = "pi pi-folder",
                            leaf = false,
                            Style = "font-weight: normal;",
                            Data = frm
                        };
                        var opcionesDelFormulario = listaOpciones
                            .Where(x => x.FORMULARIO != null && frm.FORMULARIO != null && x.FORMULARIO.Trim() == frm.FORMULARIO.Trim() && x.MODULO == frm.MODULO)
                            .ToList();

                        foreach (var opcion in opcionesDelFormulario)
                        {
                            string color = "";
                            if (opcion.PermisoEstado == "A")
                            {
                                color = "color:#3FB652; font-weight: bold;";
                            }
                            else if (opcion.PermisoEstado == "R")
                            {
                                color = "color:#FF5B5B; font-weight: bold;";
                            }
                            // If PermisoEstado == "Z" or any other value, color remains ""

                            var opc = new PrimeTreeDto
                            {
                                Children = [],
                                Expanded = false,
                                Key = opcion.COD_OPCION.ToString(),
                                Label = opcion.OPCION_DESCRIPCION.ToString(),
                                Selectable = true,
                                ExpandedIcon = "fa-solid fa-pager",
                                CollapsedIcon = "fa-solid fa-pager",
                                leaf = true,
                                Style = color,
                                Data = opcion
                            };

                            frmC.Children.Add(opc);

                        }

                        mod.Children.Add(frmC);

                    }
                    Arbol.Add(mod);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return Arbol;
        }

        public List<UsRolDto> ObtenerUsRoles()
        {
            List<UsRolDto> resultado = new List<UsRolDto>();
            try
            {
                var lista = DerechosDB.ObtenerUsRoles();

                foreach (var item in lista)
                {
                    resultado.Add(new UsRolDto
                    {
                        COD_ROL = item.COD_ROL,
                        DESCRIPCION = item.DESCRIPCION,
                        ACTIVO = item.ACTIVO,
                        REGISTRO_FECHA = item.REGISTRO_FECHA,
                        REGISTRO_USUARIO = item.REGISTRO_USUARIO,
                        COD_EMPRESA = item.COD_EMPRESA
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
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
                    resultado.Code = DerechosDB.CrearUsDerechosNewDTO(item);
                    if (resultado.Code != 0)
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
                    resultado.Code = 1;
                    resultado.Description += " - " + contador + " errores de " + info.Count.ToString() + " durante el guardado";
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resultado.Code = 1;
            }
            return resultado;

        }//end CrearUsDerechosNewDTO

        public ErrorDto EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            ErrorDto resultado = new ErrorDto();
            resultado.Code = 0;
            try
            {
                resultado.Code = DerechosDB.EliminarUsDerechosNewDTO(COD_OPCION, ESTADO, COD_ROL);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;

        }//end EliminarUsDerechosNewDTO

        public ErrorDto EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            ErrorDto resultado = new ErrorDto();
            resultado.Code = 0;
            try
            {
                resultado.Code = DerechosDB.EditarUsDerechosNew(COD_OPCION, ESTADO, COD_ROL, NUEVO_ESTADO);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }
    }
}

using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsFormulariosBl
    {
        readonly FrmUsFormulariosDb FormulariosDB;

        public FrmUsFormulariosBl(IConfiguration config)
        {
            FormulariosDB = new FrmUsFormulariosDb(config);
        }

        public List<FormularioDto> FormulariosObtener(int moduloId)
        {
            List<FormularioDto> resultado = new List<FormularioDto>();
            try
            {
                var listaFormularios = FormulariosDB.ObtenerFormulariosPorModulo(moduloId);

                foreach (var item in listaFormularios)
                {
                    resultado.Add(new FormularioDto
                    {
                        Nombre = item.Formulario,
                        Descripcion = item.Descripcion,
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;
        }

        public ErrorDto Formulario_Eliminar(int modulo, string formulario)
        {
            return FormulariosDB.Formulario_Eliminar(modulo, formulario);
        }

        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            return FormulariosDB.Formulario_Guardar(request);
        }
    }
}

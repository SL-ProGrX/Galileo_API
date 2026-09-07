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

        public ErrorDto<List<FormularioDto>> FormulariosObtener(int moduloId)
        {
            var resultado = new ErrorDto<List<FormularioDto>> { Result = new List<FormularioDto>(), Code = 0 };
            try
            {
                var listaFormularios = FormulariosDB.ObtenerFormulariosPorModulo(moduloId);

                foreach (var item in listaFormularios)
                {
                    resultado.Result.Add(new FormularioDto
                    {
                        Nombre = item.Formulario,
                        Descripcion = item.Descripcion,
                    });
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }

            resultado.Description = resultado.Code == 0 ? "Ok" : resultado.Description;
            return resultado;
        }

        public ErrorDto Formulario_Eliminar(int modulo, string formulario, int codEmpresa, string usuario)
        {
            return FormulariosDB.Formulario_Eliminar(modulo, formulario, codEmpresa, usuario);
        }

        public ErrorDto Formulario_Guardar(FormularioDto request)
        {
            return FormulariosDB.Formulario_Guardar(request);
        }
    }
}

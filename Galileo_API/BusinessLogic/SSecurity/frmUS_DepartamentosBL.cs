using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic;

public sealed class FrmUsDepartamentosBl(IConfiguration configuration)
{
    public const string CodigoNoIndica = "00";
    private readonly FrmUsDepartamentosDb _departamentos = new(configuration);

    public ErrorDto<List<Departamento>> Departamento_ObtenerTodos(string? codigoActual = null, bool soloActivos = false)
    {
        if (codigoActual?.Trim().Length > 10)
            return DbHelper.CreateErrorResponse<List<Departamento>>("El código de departamento no puede superar 10 caracteres.");

        return DbHelper.CreateOkResponse(_departamentos.ObtenerTodos(codigoActual?.Trim(), soloActivos));
    }

    public ErrorDto Departamento_Guardar(DepartamentoRequest request, string actor, bool insertar)
    {
        var validationError = Validar(request);
        if (validationError is not null)
            return DbHelper.ErrorResponse(validationError);

        if (!insertar && request.Cod_Departamento == CodigoNoIndica && request.Activo != 1)
            return DbHelper.ErrorResponse("El departamento 00 (No Indica) debe permanecer activo.");

        return _departamentos.Guardar(request, actor, insertar);
    }

    public ErrorDto Departamento_Eliminar(string codigo)
    {
        codigo = codigo?.Trim() ?? string.Empty;
        if (codigo.Length is 0 or > 10)
            return DbHelper.ErrorResponse("El código de departamento es requerido y no puede superar 10 caracteres.");
        if (codigo == CodigoNoIndica)
            return DbHelper.ErrorResponse("El departamento 00 (No Indica) no se puede eliminar.");

        return _departamentos.Eliminar(codigo);
    }

    public static string? Validar(DepartamentoRequest request)
    {
        request.Cod_Departamento = request.Cod_Departamento?.Trim() ?? string.Empty;
        request.Descripcion = request.Descripcion?.Trim() ?? string.Empty;
        if (request.Cod_Departamento.Length is 0 or > 10)
            return "El código de departamento es requerido y no puede superar 10 caracteres.";
        if (request.Descripcion.Length is 0 or > 100)
            return "La descripción es requerida y no puede superar 100 caracteres.";
        if (request.Activo is not (0 or 1))
            return "El estado del departamento no es válido.";
        return null;
    }
}

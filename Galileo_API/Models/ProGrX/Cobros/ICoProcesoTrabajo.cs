namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Datos comunes de los trabajos encolados de procesos de aplicación de Cobros.
    /// </summary>
    public interface ICoProcesoTrabajo
    {
        Guid ProcesoId { get; }

        int CodEmpresa { get; }
    }
}

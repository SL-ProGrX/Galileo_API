using Galileo.DataBaseTier.ProGrX_Beneficios;
using System.Reflection;
using Xunit;

namespace Galileo_API.Tests.ProGrX.Beneficios;

public sealed class FrmAfBeneficiosIntegralGenGuardarTests
{
    private const BindingFlags PrivadoEstatico =
        BindingFlags.NonPublic | BindingFlags.Static;

    [Fact]
    public void GuardaBeneficio_NoRecibeParametrosIgnorados()
    {
        var metodo = typeof(FrmAfBeneficiosIntegralGenDB).GetMethod(
            "Guarda_Beneficio",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(metodo);
        Assert.Equal(2, metodo.GetParameters().Length);
    }

    [Theory]
    [InlineData("P", "Producto")]
    [InlineData("M", "Monetario")]
    [InlineData("X", "Mixto")]
    public void ObtenerDescripcionTipo_ResuelveDescripcionSinTernarioAnidado(
        string tipo,
        string esperado)
    {
        var metodo = typeof(FrmAfBeneficiosIntegralGenDB).GetMethod(
            "ObtenerDescripcionTipo",
            PrivadoEstatico);

        Assert.NotNull(metodo);
        Assert.Equal(esperado, metodo.Invoke(null, new object[] { tipo }));
    }

    [Theory]
    [InlineData(100f, 100.001f, false)]
    [InlineData(100f, 100.01f, true)]
    [InlineData(100f, null, true)]
    public void MontoFueModificado_UsaToleranciaMonetaria(
        float anterior,
        float? nuevo,
        bool esperado)
    {
        var metodo = typeof(FrmAfBeneficiosIntegralGenDB).GetMethod(
            "MontoFueModificado",
            PrivadoEstatico);

        Assert.NotNull(metodo);
        Assert.Equal(esperado, metodo.Invoke(null, new object?[] { anterior, nuevo }));
    }
}

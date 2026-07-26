using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de la Tabla de Coberturas Fosol (frmFSL_TablaCoberturas).
    /// </summary>
    public class FrmFslTablaCoberturasBL
    {
        private readonly FrmFslTablaCoberturasDB _db;

        public FrmFslTablaCoberturasBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslTablaCoberturasDB(config);
        }

        /// <summary>Tabla de aplicación (coberturas) por tipo.</summary>
        public ErrorDto<FslTablaAplicacionDataLista> TablaAplicacion_Obtener(int CodCliente, string filtros)
            => _db.TablaAplicacion_Obtener(CodCliente, filtros);

        /// <summary>Guarda una cobertura (inserta o actualiza).</summary>
        public ErrorDto Cobertura_Guardar(int CodCliente, FslTablaAplicacionData aplicacion)
            => _db.Cobertura_Guardar(CodCliente, aplicacion);

        /// <summary>Elimina una cobertura.</summary>
        public ErrorDto TablaAplicacion_Eliminar(int CodCliente, string tipo, int linea)
            => _db.TablaAplicacion_Eliminar(CodCliente, tipo, linea);
    }
}

using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de la Tabla de Devoluciones Fosol (frmFSL_TablaDevoluciones).
    /// </summary>
    public class FrmFslTablaDevolucionesBL
    {
        private readonly FrmFslTablaDevolucionesDB _db;

        public FrmFslTablaDevolucionesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslTablaDevolucionesDB(config);
        }

        /// <summary>Catálogo de tipos de garantía.</summary>
        public ErrorDto<List<FslGarantiasData>> FslGarantias_Obtener(int CodCliente)
            => _db.FslGarantias_Obtener(CodCliente);

        /// <summary>Lista de devoluciones.</summary>
        public ErrorDto<FslDevolucionesDataLista> FslDevoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _db.FslDevoluciones_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Guarda una devolución (inserta o actualiza).</summary>
        public ErrorDto ParametroDevolucion_Guardar(int CodCliente, FslDevolucionesData devolucion)
            => _db.ParametroDevolucion_Guardar(CodCliente, devolucion);

        /// <summary>Elimina una devolución.</summary>
        public ErrorDto FslDevolucion_Eliminar(int CodCliente, int cod_devolucion)
            => _db.FslDevolucion_Eliminar(CodCliente, cod_devolucion);
    }
}

using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los catálogos de Tipos Fosol (frmFSL_TablasTipos).
    /// </summary>
    public class FrmFslTablasTiposBL
    {
        private readonly FrmFslTablasTiposDB _db;

        public FrmFslTablasTiposBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslTablasTiposDB(config);
        }

        /// <summary>Lista de tipos (gestiones, apelaciones o enfermedades).</summary>
        public ErrorDto<FslTablaTipoLista> FslTablaTipos_Obtener(int CodCliente, string tipo, string? filtro, int? pagina, int? paginacion)
            => _db.FslTablaTipos_Obtener(CodCliente, tipo, filtro, pagina, paginacion);

        /// <summary>Actualiza un tipo.</summary>
        public ErrorDto FslTablaTipos_Actualizar(int CodCliente, string tipo, FslTablaTipoData tipoData)
            => _db.FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);

        /// <summary>Inserta un tipo (o actualiza si existe).</summary>
        public ErrorDto FslTablaTipo_Insertar(int CodCliente, string tipo, string usuario, FslTablaTipoData tipoData)
            => _db.FslTablaTipo_Insertar(CodCliente, tipo, usuario, tipoData);

        /// <summary>Elimina un tipo.</summary>
        public ErrorDto FslTablaTipo_Eliminar(int CodCliente, string tipo, string codigo)
            => _db.FslTablaTipo_Eliminar(CodCliente, tipo, codigo);
    }
}

using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de Remesas de Pago Fosol (frmFSL_RemesasPago).
    /// </summary>
    public class FrmFslRemesasPagoBL
    {
        private readonly FrmFslRemesasPagoDB _db;

        public FrmFslRemesasPagoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslRemesasPagoDB(config);
        }

        /// <summary>Fechas de una remesa de tesorería.</summary>
        public ErrorDto<List<FslRemesasListaDatos>> FslFechas_Obtener(int CodEmpresa, int cod_remesa)
            => _db.FslFechas_Obtener(CodEmpresa, cod_remesa);

        /// <summary>Lista de remesas de tesorería.</summary>
        public ErrorDto<FslRemesasLista> FslRemesas_Obtener(int CodEmpresa, string? filtro, int? pagina, int? paginacion)
            => _db.FslRemesas_Obtener(CodEmpresa, filtro, pagina, paginacion);

        /// <summary>Inserta una remesa de tesorería.</summary>
        public ErrorDto FslRemesa_Agregar(int CodEmpresa, FslRemesaInsertar remesa)
            => _db.FslRemesa_Agregar(CodEmpresa, remesa);

        /// <summary>Actualiza una remesa de tesorería.</summary>
        public ErrorDto FslRemesa_Actualizar(int CodEmpresa, FslRemesaInsertar remesa)
            => _db.FslRemesa_Actualizar(CodEmpresa, remesa);

        /// <summary>Elimina una remesa de tesorería.</summary>
        public ErrorDto FslRemesa_Eliminar(int CodEmpresa, int cod_remesa)
            => _db.FslRemesa_Eliminar(CodEmpresa, cod_remesa);

        /// <summary>Cierra una remesa de tesorería.</summary>
        public ErrorDto FslRemesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
            => _db.FslRemesa_Cerrar(CodEmpresa, cod_remesa, usuario);

        /// <summary>Remesas abiertas para cargas.</summary>
        public ErrorDto<List<FslRemesasListaDatos>> FslCargas_Obtener(int CodEmpresa)
            => _db.FslCargas_Obtener(CodEmpresa);

        /// <summary>Expedientes elegibles para carga.</summary>
        public ErrorDto<FslCargasLista> FslCargasLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, string? filtro, int? pagina, int? paginacion)
            => _db.FslCargasLista_Obtener(CodEmpresa, fecha_inicio, fecha_corte, filtro, pagina, paginacion);

        /// <summary>Aplica una remesa a los expedientes seleccionados.</summary>
        public ErrorDto FslCargas_Aplicar(int CodEmpresa, string cargas)
            => _db.FslCargas_Aplicar(CodEmpresa, cargas);

        /// <summary>Cierra una remesa de cargas.</summary>
        public ErrorDto FslCargas_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
            => _db.FslCargas_Cerrar(CodEmpresa, cod_remesa, usuario);

        /// <summary>Remesas cerradas listas para trasladar.</summary>
        public ErrorDto<List<FslRemesasListaDatos>> FslTraslados_Obtener(int CodEmpresa)
            => _db.FslTraslados_Obtener(CodEmpresa);

        /// <summary>Expedientes de una remesa pendientes de traslado.</summary>
        public ErrorDto<List<FslTrasladoListaData>> FslTrasladoLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, int cod_remesa)
            => _db.FslTrasladoLista_Obtener(CodEmpresa, fecha_inicio, fecha_corte, cod_remesa);

        /// <summary>Aplica el traslado a tesorería de los expedientes de una remesa.</summary>
        public ErrorDto FslTraslado_Aplicar(int CodEmpresa, string traslados)
            => _db.FslTraslado_Aplicar(CodEmpresa, traslados);
    }
}

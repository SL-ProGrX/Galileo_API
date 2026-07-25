using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Traslado de Beneficios a Tesorería (frmAF_BeneficiosTraslado).
    /// </summary>
    public class FrmAfBeneficiosTrasladoBL
    {
        private readonly FrmAfBeneficiosTrasladoDB _db;

        public FrmAfBeneficiosTrasladoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosTrasladoDB(config);
        }

        /// <summary>Lista de remesas de traslado.</summary>
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiRemesas_Obtener(int CodCliente, string filtros)
            => _db.AfiRemesas_Obtener(CodCliente, filtros);

        /// <summary>Remesa de traslado por código.</summary>
        public ErrorDto<AfiBeneficiosRemesasDto> AfiRemesa_Obtener(int CodCliente, int cod_remesa)
            => _db.AfiRemesa_Obtener(CodCliente, cod_remesa);

        /// <summary>Oficinas con créditos pendientes de traslado en un rango de fechas.</summary>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinasFechas_Obtener(int CodCliente, string inicio, string corte)
            => _db.AfiRemesaOficinasFechas_Obtener(CodCliente, inicio, corte);

        /// <summary>Catálogo completo de oficinas.</summary>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinas_Obtener(int CodCliente)
            => _db.AfiRemesaOficinas_Obtener(CodCliente);

        /// <summary>Bancos con pagos pendientes de traslado.</summary>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBancos_Obtener(int CodCliente, string inicio, string corte)
            => _db.CargarBancos_Obtener(CodCliente, inicio, corte);

        /// <summary>Usuarios con pagos pendientes de traslado.</summary>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarUsuarios_Obtener(int CodCliente, string inicio, string corte)
            => _db.CargarUsuarios_Obtener(CodCliente, inicio, corte);

        /// <summary>Beneficios con pagos pendientes de traslado.</summary>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBeneficios_Obtener(int CodCliente)
            => _db.CargarBeneficios_Obtener(CodCliente);

        /// <summary>Cargas de beneficios pendientes de traslado.</summary>
        public ErrorDto<AfiBeneficiosCargasDataLista> BusquedaCargas_Obtener(int CodCliente, string filtros)
            => _db.BusquedaCargas_Obtener(CodCliente, filtros);

        /// <summary>Remesas abiertas para cargas.</summary>
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiCargasRemesas_Obtener(int CodCliente)
            => _db.AfiCargasRemesas_Obtener(CodCliente);

        /// <summary>Remesas cerradas listas para trasladar.</summary>
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiTraslados_Obtener(int CodCliente)
            => _db.AfiTraslados_Obtener(CodCliente);

        /// <summary>Beneficios de una remesa lista para traslado.</summary>
        public ErrorDto<AfiBeneficiosCargasDataLista> AfiTraslado_Obtener(int CodCliente, string filtros)
            => _db.AfiTraslado_Obtener(CodCliente, filtros);

        /// <summary>Tokens disponibles para la liquidación.</summary>
        public ErrorDto<List<TokenConsultaModel>> Afi_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
            => _db.Afi_LiqAsientosToken_Obtener(CodEmpresa, usuario);

        /// <summary>Informe superior de remesas.</summary>
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiInformesTop_Obtener(int CodCliente, string filtros)
            => _db.AfiInformesTop_Obtener(CodCliente, filtros);

        /// <summary>Cubo de beneficios (consulta detallada) por rango de fechas.</summary>
        public ErrorDto<List<CuboBeneficiosData>> Cubo_Beneficios_Obtener(int CodCliente, CuboParametros remesa)
            => _db.Cubo_Beneficios_Obtener(CodCliente, remesa);

        /// <summary>Inserta una remesa de traslado.</summary>
        public ErrorDto AfiRemesa_Insertar(int CodCliente, AfiBeneficiosRemesasDto remesa)
            => _db.AfiRemesa_Insertar(CodCliente, remesa);

        /// <summary>Actualiza una remesa de traslado.</summary>
        public ErrorDto AfiRemesa_Actualizar(int CodCliente, AfiBeneficiosRemesasDto remesa)
            => _db.AfiRemesa_Actualizar(CodCliente, remesa);

        /// <summary>Elimina una remesa de traslado.</summary>
        public ErrorDto AfiRemesa_Eliminar(int CodCliente, long cod_remesa)
            => _db.AfiRemesa_Eliminar(CodCliente, cod_remesa);

        /// <summary>Aplica una remesa a los beneficios seleccionados.</summary>
        public ErrorDto CargaCarga_Aplicar(int CodCliente, string carga)
            => _db.CargaCarga_Aplicar(CodCliente, carga);

        /// <summary>Cierra una remesa de traslado.</summary>
        public ErrorDto CargasCarga_Cerrar(int CodCliente, string cod_remesa, string usuario)
            => _db.CargasCarga_Cerrar(CodCliente, cod_remesa, usuario);

        /// <summary>Aplica el traslado a tesorería de los beneficios de una remesa.</summary>
        public Task<ErrorDto> AfiTraslado_Aplicar(int CodCliente, string traslado)
            => _db.AfiTraslado_Aplicar(CodCliente, traslado);

        /// <summary>Genera un nuevo token para la liquidación.</summary>
        public ErrorDto Afi_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
            => _db.Afi_LiqAsientoToken_Nuevo(CodEmpresa, usuario);
    }
}

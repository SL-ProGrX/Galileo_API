using Galileo.Models;
using Galileo.Models.AF;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Orden de Pago de Beneficios Integrales (FrmAfBeneficiosIntegralOrP).
    /// Constructor y dependencias compartidas. Catálogos, consultas, orden de pago y proyección en parciales.
    /// </summary>
    public partial class FrmAfBeneficiosIntegralOrPDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;

        /// <summary>
        /// Inicializa el acceso a datos y las dependencias (bitácora, validaciones, auxiliares).
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralOrPDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
            _AuxiliarDB = new MProGrXAuxiliarDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Construye el modelo de validación de beneficio a partir de la orden de pago recibida.
        /// </summary>
        private static BeneficioGeneralDatos MapBeneficioValida(AfiBeneIntegralOrP beneficio)
        {
            var datos = new BeneficioGeneralDatos
            {
                cedula = beneficio.cedula.Trim(),
                monto_aplicado = beneficio.monto,
                registra_user = beneficio.registro_usuario,
                estado = new AfBeneficioIntegralDropsLista { item = beneficio.estado },
                consec = beneficio.consec
            };
            datos.cod_beneficio.item = beneficio.cod_beneficio;
            return datos;
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de beneficios (helper compartido).
        /// </summary>
        private void RegistrarBitacora(int CodCliente, string codBeneficio, int consec, string? usuario, string movimiento, string detalle)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = consec,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = (usuario ?? string.Empty).ToUpper()
            });
        }
    }
}

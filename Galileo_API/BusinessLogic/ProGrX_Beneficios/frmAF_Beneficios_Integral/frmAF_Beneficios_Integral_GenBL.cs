using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Generales de Beneficios Integrales (FrmAfBeneficiosIntegralGen).
    /// </summary>
    public class FrmAfBeneficiosIntegralGenBL
    {
        private readonly FrmAfBeneficiosIntegralGenDB _db;

        public FrmAfBeneficiosIntegralGenBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralGenDB(config);
        }

        /// <summary>Lista de beneficios de una categoría.</summary>
        public ErrorDto<List<BeneficiosLista>> BeneficiosLista_Obtener(int CodCliente, string categoria)
            => _db.BeneficiosLista_Obtener(CodCliente, categoria);

        /// <summary>Productos asignados al beneficio.</summary>
        public ErrorDto<List<AfiBenProductoDto>> BeneIntegralGenProductos_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _db.BeneIntegralGenProductos_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Datos generales del beneficio.</summary>
        public ErrorDto<BeneficioGeneral> BeneficioIntegralGeneral_Obtener(int CodCliente, int? id_beneficio)
            => _db.BeneficioIntegralGeneral_Obtener(CodCliente, id_beneficio);

        /// <summary>[PENDIENTE] Guardado central del beneficio.</summary>
        public Task<ErrorDto<BeneficioGeneralDatos>> BeneficioIntegralGeneral_Guardar(int CodCliente, string fuente, BeneficioGeneralDatos beneficio)
            => _db.BeneficioIntegralGeneral_Guardar(CodCliente, fuente, beneficio);

        /// <summary>Valida si el socio ya está en el programa Crece.</summary>
        public ErrorDto ValidaProgramaCrece(int CodCliente, string cedula)
            => _db.ValidaProgramaCrece(CodCliente, cedula);

        /// <summary>[PENDIENTE] Notificación de resolución por correo.</summary>
        public Task<ErrorDto> BeneficioNotificaResolucion_Enviar(List<DocArchivoBeneIntegralDto> parametros)
            => _db.BeneficioNotificaResolucion_Enviar(parametros);

        /// <summary>Valida si el estado es de resolución del expediente.</summary>
        public ErrorDto ValidaEstadoExpediente(int CodCliente, string estado, string categoria)
            => _db.ValidaEstadoExpediente(CodCliente, estado, categoria);

        /// <summary>Lista de profesionales APT.</summary>
        public ErrorDto<List<BeneApreLista>> AfiBeneProfesionales_Obtener(int CodCliente)
            => _db.AfiBeneProfesionales_Obtener(CodCliente);

        /// <summary>Lista de categorías APT.</summary>
        public ErrorDto<List<BeneApreLista>> AfiBeneCategorias_Obtener(int CodCliente)
            => _db.AfiBeneCategorias_Obtener(CodCliente);

        /// <summary>Valida si el beneficio requiere justificación.</summary>
        public ErrorDto ValidaRequiereJustificacion(int CodCliente, string cedula, string beneficio)
            => _db.ValidaRequiereJustificacion(CodCliente, cedula, beneficio);

        /// <summary>Tipo de beneficio.</summary>
        public ErrorDto<string> ValidaTipoBeneficio(int CodCliente, string? cod_beneficio)
            => _db.ValidaTipoBeneficio(CodCliente, cod_beneficio);

        /// <summary>Registro de mora del beneficio.</summary>
        public ErrorDto<BeneRegistroMoraDto> BeneRegistroMora_Obtener(int CodCliente, int consec, string beneficio)
            => _db.BeneRegistroMora_Obtener(CodCliente, consec, beneficio);

        /// <summary>Guarda el registro de mora del beneficio.</summary>
        public ErrorDto BeneRegistroMora_Guardar(int CodCliente, BeneRegistroMoraGuardar cobroMora)
            => _db.BeneRegistroMora_Guardar(CodCliente, cobroMora);

        /// <summary>[PENDIENTE] Envío de boleta de cobro de mora por correo.</summary>
        public Task<ErrorDto> BeneRegistroMora_Enviar(int CodCliente, DocArchivoBeneIntegralDto parametros)
            => _db.BeneRegistroMora_Enviar(CodCliente, parametros);

        /// <summary>Valida si la persona figura como fallecida.</summary>
        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
            => _db.ValidaFallecido(CodCliente, cedulafallecido);
    }
}

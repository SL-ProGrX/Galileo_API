using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del formulario principal de Beneficios Integrales (frmAF_Beneficios_Integral).
    /// </summary>
    public class FrmAfBeneficiosIntegralBL
    {
        private readonly FrmAfBeneficiosIntegralDB _db;

        public FrmAfBeneficiosIntegralBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralDB(config);
        }

        /// <summary>Catálogos de tablas SYS/BENE.</summary>
        public ErrorDto<List<CatalogosLista>> Catalogo_Obtener(int CodEmpresa, int tipo, int modulo)
            => _db.Catalogo_Obtener(CodEmpresa, tipo, modulo);

        /// <summary>Lista de categorías de beneficios.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneIntegralCategorias_Obtener(int CodCliente)
            => _db.BeneIntegralCategorias_Obtener(CodCliente);

        /// <summary>Observaciones del beneficio.</summary>
        public ErrorDto<List<AfiBeneObservaciones>> BeneIntegralObservaciones_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _db.BeneIntegralObservaciones_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda una observación del beneficio.</summary>
        public ErrorDto BeneIntegralObservaciones_Guardar(int CodCliente, AfiBeneObservaciones observacion)
            => _db.BeneIntegralObservaciones_Guardar(CodCliente, observacion);

        /// <summary>Elimina una observación del beneficio.</summary>
        public ErrorDto BeneIntegralObservaciones_Eliminar(int CodCliente, int id_observacion, string usuario)
            => _db.BeneIntegralObservaciones_Eliminar(CodCliente, id_observacion, usuario);

        /// <summary>Bitácora del beneficio.</summary>
        public ErrorDto<List<BitacoraBeneficioIntegralDto>> BitacoraBeneficioIntegral_Obtener(int CodCliente, string Cod_Beneficio, int Consec)
            => _db.BitacoraBeneficioIntegral_Obtener(CodCliente, Cod_Beneficio, Consec);

        /// <summary>Expediente del beneficio (tablas serializadas en JSON).</summary>
        public ErrorDto<object> BeneIntegralRepExpediente_Obtener(int CodEmpresa, string cedula, int id_beneficio, string categoria)
            => _db.BeneIntegralRepExpediente_Obtener(CodEmpresa, cedula, id_beneficio, categoria);

        /// <summary>Beneficios para aprobación masiva.</summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosParaAprobacionMasiva_Obtener(int CodEmpresa, string Categoria, string filtroString)
            => _db.BeneficiosParaAprobacionMasiva_Obtener(CodEmpresa, Categoria, filtroString);

        /// <summary>Aprueba de forma masiva los beneficios seleccionados.</summary>
        public ErrorDto BeneIntegral_AprobacionMasiva(int CodEmpresa, string lista)
            => _db.BeneIntegral_AprobacionMasiva(CodEmpresa, lista);

        /// <summary>Beneficios para control mensual.</summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Obtener(int CodEmpresa, string Categoria, string filtroString)
            => _db.BeneficiosControMensual_Obtener(CodEmpresa, Categoria, filtroString);

        /// <summary>Genera las solicitudes de depósito.</summary>
        public ErrorDto BeneSolicitudDeposito_Generar(int CodEmpresa, string lista, int mes)
            => _db.BeneSolicitudDeposito_Generar(CodEmpresa, lista, mes);

        /// <summary>Devuelve las solicitudes de depósito.</summary>
        public ErrorDto BeneSolicitudDeposito_Devolver(int CodEmpresa, string lista)
            => _db.BeneSolicitudDeposito_Devolver(CodEmpresa, lista);

        /// <summary>Reporte de control mensual.</summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Reporte(int CodEmpresa, string Categoria, string filtroString)
            => _db.BeneficiosControMensual_Reporte(CodEmpresa, Categoria, filtroString);

        /// <summary>Grupos de beneficios de una categoría.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneficioGrupos_Obtener(int CodEmpresa, string Categoria)
            => _db.BeneficioGrupos_Obtener(CodEmpresa, Categoria);

        /// <summary>Permisos del usuario para la categoría.</summary>
        public ErrorDto<BeneCategoriaPermisos> ValidaUsuarioBeneficios_Obtener(int CodEmpresa, string usuario, string cod_categoria)
            => _db.ValidaUsuarioBeneficios_Obtener(CodEmpresa, usuario, cod_categoria);

        /// <summary>Envía la solicitud de bloqueo del asociado al Departamento de Cobros.</summary>
        public async Task<ErrorDto> BeneSolicitudBloqueo_Enviar(int CodCliente, DocArchivoBeneIntegralDto parametros)
            => await _db.BeneSolicitudBloqueo_Enviar(CodCliente, parametros);
    }
}

using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Mantenimiento de Beneficios (frmAF_Beneficios).
    /// </summary>
    public class FrmAfBeneficiosBL
    {
        private readonly FrmAfBeneficiosDB _db;

        public FrmAfBeneficiosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosDB(config);
        }

        /// <summary>Navegación (scroll) de beneficios.</summary>
        public ErrorDto Top1Beneficio_Obtener(int CodCliente, int Scroll, string Cod_Beneficio)
            => _db.Top1Beneficio_Obtener(CodCliente, Scroll, Cod_Beneficio);

        /// <summary>Detalle de un beneficio.</summary>
        public ErrorDto<AfiBeneficiosDto> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
            => _db.AfiBeneficioDTO_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Montos configurados de un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioMontoData>> AfiBeneficioMontos_Obtener(int CodCliente, string Cod_Beneficio)
            => _db.AfiBeneficioMontos_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Grupos y su marca de asignación a un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioGruposData>> AfiBeneficioGrupos_Obtener(int CodCliente, string Cod_Beneficio)
            => _db.AfiBeneficioGrupos_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Nombre de una cuenta contable.</summary>
        public ErrorDto NombreCuenta_Obtener(int CodCliente, string cuenta)
            => _db.NombreCuenta_Obtener(CodCliente, cuenta);

        /// <summary>Catálogo de categorías de beneficios activas.</summary>
        public ErrorDto<List<AfiBeneListas>> AfiBeneCategoria_Obtener(int CodCliente)
            => _db.AfiBeneCategoria_Obtener(CodCliente);

        /// <summary>Grupos de una categoría de beneficios.</summary>
        public ErrorDto<List<AfiBeneListas>> AfiBeneGrupos_Obtener(int CodCliente, string categoria)
            => _db.AfiBeneGrupos_Obtener(CodCliente, categoria);

        /// <summary>Bitácora de un beneficio.</summary>
        public ErrorDto<List<BitacoraBeneficioDto>> BitacoraBeneficio_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec, string? cod_grupo, string? cod_categoria)
            => _db.BitacoraBeneficio_Obtener(CodEmpresa, Cod_Beneficio, Consec, cod_grupo, cod_categoria);

        /// <summary>Fechas de pago automático de un beneficio.</summary>
        public ErrorDto<List<AfiBeneFechaPagoData>> AfiBeneFechasPago_Obtener(int CodCliente, string Cod_Beneficio, int Periodo)
            => _db.AfiBeneFechasPago_Obtener(CodCliente, Cod_Beneficio, Periodo);

        /// <summary>Actualiza un beneficio.</summary>
        public ErrorDto AfiBeneficios_Actualiza(int CodCliente, AfiBeneficiosDto Beneficio)
            => _db.AfiBeneficios_Actualiza(CodCliente, Beneficio);

        /// <summary>Inserta un beneficio.</summary>
        public ErrorDto AfiBeneficios_Insertar(int CodCliente, AfiBeneficiosDto Beneficio)
            => _db.AfiBeneficios_Insertar(CodCliente, Beneficio);

        /// <summary>Elimina un beneficio.</summary>
        public ErrorDto AfiBeneficios_Eliminar(int CodCliente, string Cod_Beneficio)
            => _db.AfiBeneficios_Eliminar(CodCliente, Cod_Beneficio);

        /// <summary>Asocia un grupo a un beneficio.</summary>
        public ErrorDto AfiBeneGruposB_Insertar(int CodCliente, string cod_grupo, string cod_beneficio)
            => _db.AfiBeneGruposB_Insertar(CodCliente, cod_grupo, cod_beneficio);

        /// <summary>Desasocia un grupo de un beneficio.</summary>
        public ErrorDto AfiBeneGruposB_Eliminar(int CodCliente, string cod_grupo, string cod_beneficio)
            => _db.AfiBeneGruposB_Eliminar(CodCliente, cod_grupo, cod_beneficio);

        /// <summary>Guarda un monto de beneficio (inserta o actualiza).</summary>
        public ErrorDto AfiBeneficioMontos_Guardar(int CodCliente, AfiBeneficioMontoData Monto)
            => _db.AfiBeneficioMontos_Guardar(CodCliente, Monto);

        /// <summary>Elimina un monto de beneficio.</summary>
        public ErrorDto AfiBeneficioMontos_Eliminar(int CodCliente, int id_bene, string cod_beneficio)
            => _db.AfiBeneficioMontos_Eliminar(CodCliente, id_bene, cod_beneficio);

        /// <summary>Guarda las fechas de pago automático.</summary>
        public ErrorDto AfiBeneFechasPago_Guardar(int CodCliente, List<AfiBeneFechaPagoData> DataFechas, string Usuario)
            => _db.AfiBeneFechasPago_Guardar(CodCliente, DataFechas, Usuario);
    }
}

using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfCuentasDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfCuentasDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmArfCuentasDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Divisas_Obtener(int codEmpresa)
        {
            string query = @"select COD_DIVISA as item, DESCRIPCION from vSys_Divisas";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Unidades_Obtener(int codEmpresa)
        {
            string query = @"select cod_Unidad as item, DESCRIPCION from vARF_UNIDADES";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<ArfCuentasDto>> ArfCuentas_Obtener(int codEmpresa, string codDivisa, string codUnidad)
        {
            string query = @"select * from vARF_CUENTAS Where COD_DIVISA = @codDivisa and COD_UNIDAD = @codUnidad";
            return DbHelper.ExecuteListQuery<ArfCuentasDto>(_portalDb, codEmpresa, query, new { codDivisa, codUnidad });
        }

        public ErrorDto ArfCuentas_Registrar(int codEmpresa, ArfCuentasRegistraRequest req)
        {
            const string sql = @"
            exec spARF_Cuentas_Registra
                @CodUnidad,
                @CodDivisa,
                @Usuario,
                @CtaActivo,
                @CtaPasivo,
                @CtaGastoInteres,
                @CtaGastoAlquiler,
                @CtaAmortDerecho,
                @CtaAmortAcumulada,
                @CtaPuente;
            ";

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodUnidad = req.cod_unidad,
                    CodDivisa = req.cod_divisa,
                    Usuario = req.usuario,

                    CtaActivo = req.cta_activo,
                    CtaPasivo = req.cta_pasivo,
                    CtaGastoInteres = req.cta_gasto_interes,
                    CtaGastoAlquiler = req.cta_gasto_alquiler,
                    CtaAmortDerecho = req.cta_amort_derecho,
                    CtaAmortAcumulada = req.cta_amort_acumulada,
                    CtaPuente = req.cta_puente
                }
            );

            if (response != null && response.Code < 0)
                return response;

            return new ErrorDto { Code = 0, Description = "Información Actualizada Satisfactoriamente!" };
        }

    }
}
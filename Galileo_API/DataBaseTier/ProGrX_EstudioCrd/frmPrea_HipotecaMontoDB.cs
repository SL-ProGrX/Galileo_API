using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaHipotecaMontoDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaHipotecaMontoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de gastos asociados de hipoteca por tipo para el expediente indicado.
        /// Tipos soportados: TRA, CAN y CON.
        /// </summary>
        public ErrorDto<FrmPreaHipotecaMontoListaResponse> Prea_frmPreaHipotecaMonto_Lista_Obtener(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            const string sql = @"
                EXEC spCrdPrea_Hipotecas_Gastos
                    @cod_preanalisis,
                    @tipo;";

            var queryResult = DbHelper.ExecuteListQuery<FrmPreaHipotecaMontoItemData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_preanalisis = cod_preanalisis.Trim(),
                    tipo = tipo.Trim().ToUpperInvariant()
                });

            if (queryResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FrmPreaHipotecaMontoListaResponse>(queryResult.Description!);
            }

            var result = new FrmPreaHipotecaMontoListaResponse
            {
                cod_preanalisis = cod_preanalisis.Trim(),
                tipo = tipo.Trim().ToUpperInvariant(),
                lista = (queryResult.Result ?? new List<FrmPreaHipotecaMontoItemData>())
                    .Select(item => new FrmPreaHipotecaMontoItem
                    {
                        id_param = item.id_param,
                        asigna = item.asigna,
                        monto_min = item.monto_min,
                        monto_max = item.monto_max,
                        gastos = item.gastos,
                        honorarios = item.honorarios,
                        imp_traspaso = item.imp_traspaso
                    })
                    .ToList()
            };

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Guarda la selección única del gasto asociado de hipoteca para el expediente y tipo indicado.
        /// </summary>
        public ErrorDto<FrmPreaHipotecaMontoGuardarResponse> Prea_frmPreaHipotecaMonto_Seleccion_Guardar(
            int codEmpresa,
            FrmPreaHipotecaMontoGuardarRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tipo = request.tipo.Trim().ToUpperInvariant();

                int? idParamBi = null;
                int? idParamCanh = null;
                int? idParamConsh = null;
                string tipoParam = string.Empty;

                switch (tipo)
                {
                    case "TRA":
                        idParamBi = request.id_param;
                        tipoParam = "BIIM";
                        break;

                    case "CAN":
                        idParamCanh = request.id_param;
                        tipoParam = "CANH";
                        break;

                    case "CON":
                        idParamConsh = request.id_param;
                        tipoParam = "CONH";
                        break;
                }

                const string sql = @"
                    EXEC spCrdPreaAvaluosHipoteca
                        @cod_preanalisis,
                        @id_param_bi,
                        @id_param_canh,
                        @id_param_consh,
                        @tipo_param,
                        @usuario;";

                connection.Execute(
                    sql,
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        id_param_bi = idParamBi,
                        id_param_canh = idParamCanh,
                        id_param_consh = idParamConsh,
                        tipo_param = tipoParam,
                        usuario = request.usuario.Trim()
                    });

                return DbHelper.CreateOkResponse(new FrmPreaHipotecaMontoGuardarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    tipo = tipo,
                    id_param = request.id_param,
                    mensaje = "Selección guardada correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaHipotecaMontoGuardarResponse>(ex.Message);
            }
        }
    }
}

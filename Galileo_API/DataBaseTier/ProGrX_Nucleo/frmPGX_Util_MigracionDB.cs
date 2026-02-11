using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmPgxUtilMigracionDB
    {
        private readonly IConfiguration _config;

        public FrmPgxUtilMigracionDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto PGX_UtilMigracion_Aplicar(int CodEmpresa, string usuario, List<PgxMigracionData> file)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                int vComite = 1;
                string vGarantia = "N";
                string vObservaciones = "Datos Migrados";

                using var connection = new SqlConnection(stringConn);

                foreach (var row in file)
                {


                    var query = @"insert reg_creditos
                                        (codigo,id_comite,cedula,montosol,montoapr,monto_girado
                                        ,saldo,amortiza,interesc,saldo_mes,cuota,int,interesv,plazo,userrec,userres
                                        ,userfor,usertesoreria,tesoreria,fechasol,fechares,fechaforp,fechaforf
                                        ,fecha_calculo_int,garantia,primer_cuota,tdocumento,ndocumento,pagare
                                        ,firma_deudor,premio,observacion,estado,prideduc,fecult,estadosol,documento_referido,cod_destino
                                  values(@cod,@Comite,@pCedula,@pMonto,@pMonto,@pMonto,
                                         @pSaldo,@pAmortiza,0,@pSaldo,@pCuota,@pTasa,@pTasa,@pPlazo,@user,@user,
                                         @user,@user,@pFormaliza,@pFormaliza,getdate(),@pFormaliza,@pFormaliza,
                                         @pFormaliza,@Garantia,'N','OT',@pReferencia,0
                                         1,0,@Observaciones,'A',@pPriDeduc,@pFecUlt,'F',@pReferencia,Null)";

                    connection.Execute(query, new
                    {
                        cod = row.codigo,
                        Comite = vComite,
                        pCedula = row.cedula.Trim(),
                        pMonto = row.monto,
                        pSaldo = row.saldo,
                        pAmortiza = Convert.ToDecimal(row.monto) - Convert.ToDecimal(row.saldo),
                        pCuota = row.cuota,
                        pTasa = row.tasa,
                        pPlazo = row.plazo,
                        user = usuario,
                        pFormaliza = row.formaliza, //.ToString("yyyy/MM/dd"),
                        Garantia = vGarantia,
                        pReferencia = row.operacion,
                        Observaciones = vObservaciones,
                        pPriDeduc = row.pri_deduccion,
                        pFecUlt = row.fec_ult,

                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_AnulaAhorrosDB
    {
        private readonly IConfiguration _config;

        public frmAH_AnulaAhorrosDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO Aplicar_Transacciones(int CodEmpresa, TransaccionSIFDTO transaccion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);


                var query = $@"insert into afi_bene_productos(cod_producto,descripcion,costo_unidad)  
                                    values ('')";

                //                INSERT INTO SIF_TRANSACCIONES(
                //    COD_TRANSACCION,
                //    TIPO_DOCUMENTO,
                //    REGISTRO_FECHA,
                //    REGISTRO_USUARIO,
                //    Cliente_IDENTIFICACION,
                //    CLIENTE_NOMBRE,
                //    cod_concepto,
                //    monto,
                //    estado,
                //    Referencia_01,
                //    Referencia_02,
                //    Referencia_03,
                //    cod_oficina,
                //    linea1,
                //    linea2,
                //    linea3,
                //    linea4,
                //    linea5,
                //    linea6,
                //    linea7,
                //    linea8,
                //    linea9,
                //                linea10,
                //                detalle,
                //                documento,
                //                cod_caja,
                //                cod_apertura
                //) VALUES(
                //    @vNumDoc,
                //    @vTipoDoc,
                //    GETDATE(), --O la función específica para obtener la fecha actual en tu sistema
                //    @glodon_Usuario,
                //    @txtCedula,
                //    @txtNombre,
                //    'PAT001',
                //    @txtMonto, --Asegúrate de que sea un tipo numérico compatible
                //    'P',
                //    @txtCedula,
                //    '',
                //    '',
                //    @GLOBALES_gOficinaTitular,
                //    @strLinea1,
                //    @strLinea2,
                //    @strLinea3,
                //    @strLinea4,
                //    @strLinea5,
                //    @strLinea6,
                //    @strLinea7,
                //    @strLinea8,
                //    @strLinea9,
                //    @strLinea10,
                //    @vAseDocDetalle,
                //    @vAseDocDeposito,
                //    @ModuloCajas_mCaja,
                //    @ModuloCajas_mApertura
                //);
                var result = connection.Execute(query);





            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }
    }
}

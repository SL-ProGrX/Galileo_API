using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo_API.Models.ProGrX.Bancos;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesConsultaCuentaSinpeDB
    {
        private readonly PortalDB _portalDB;
        private readonly VerificadorCoreFactory _factory;

        public FrmTesConsultaCuentaSinpeDB(IConfiguration config)
        {
            _factory = new VerificadorCoreFactory(config);
            _portalDB = new PortalDB(config);
        }

        public ErrorDto Tes_ConsultaCuentasSinpe_Aplicar(int CodEmpresa, int aplica, TesConsultaCuentaSinpeModels cuenta)
        {
            switch(aplica)
            {
                case 1: //Valida Cuentas SINPE
                    return validaCuentasSinpe(CodEmpresa, cuenta);
                case 4: //Eliminar Cuentas Cerradas
                    return eliminarCuentasCerradas(CodEmpresa, cuenta);
                default:
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Parámetro 'aplica' inválido."
                    };
            }
        }

        private ErrorDto validaCuentasSinpe(int CodEmpresa, TesConsultaCuentaSinpeModels cuenta)
        {
            try
            {
               
                return _factory.CrearServicio(CodEmpresa, cuenta.usuario).ConsultaCuentaSinpe(CodEmpresa, cuenta);
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al validar cuentas SINPE: {ex.Message}"
                };
            }
        }

        private ErrorDto eliminarCuentasCerradas(int CodEmpresa, TesConsultaCuentaSinpeModels cuenta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            if (cuenta.error != 23)
            {
                return DbHelper.ErrorResponse($"La cuenta no esta en estado Cerrada");
            }

            string vCedula = cuenta.cedula.Trim().Replace("-","");

            var parameters = new
            {
                Cedula = vCedula,
                CuentaIBAN = cuenta.cuentaIban,
                Banco = cuenta.idBanco
            };

            //valida si existe:
            const string query = @"
                        SELECT COUNT(*) 
                        FROM SYS_CUENTAS_BANCARIAS
                        WHERE REPLACE(identificacion, '-', '') = @Cedula
                          AND Cuenta_interna = @CuentaIBAN
                          AND Cod_banco = @Banco";

            var existe = conn.Query<int>(query, parameters).FirstOrDefault();
            if (existe == 0)
            {
                return DbHelper.ErrorResponse($"Error al verificar existencia de cuenta");
            }

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                try
                {
                    string sql = @"
                        UPDATE SYS_CUENTAS_BANCARIAS SET Activa = 0
                        WHERE REPLACE(identificacion, '-', '') = @Cedula
                          AND Cuenta_interna = @CuentaIBAN
                          AND Cod_banco = @Banco";
                    
                    var result = DbHelper.ExecuteNonQuery(conn.ConnectionString, sql, parameters);
                    if (result.Code != 0)
                    {
                        return DbHelper.ErrorResponse($"Error al desactivar cuentas cerradas: {result.Description}");
                    }
                    return DbHelper.OkResponse("Cuenta Desactivada");
                }
                catch (Exception ex)
                {
                    return DbHelper.ErrorResponse($"Excepción al desactivar cuentas cerradas: {ex.Message}");
                }
            }).Result ?? DbHelper.ErrorResponse("Error desconocido");
        }

    }
}

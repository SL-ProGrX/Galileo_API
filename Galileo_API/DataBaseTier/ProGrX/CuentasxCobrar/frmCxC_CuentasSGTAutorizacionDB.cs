using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasSGTAutorizacionModels;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSGTAutorizacionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCxC = 31; 
        private const string MovAplica = "APLICA - WEB";


        public FrmCxCCuentasSGTAutorizacionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
       
        }
        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        /// <summary>
        /// Consulta de datos de cuenta SGT por operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CuentasSGTAutorizacionDto?> CxCCuentasSGTAutorizacion_Consulta(int codEmpresa,int operacion)
        {
            var query = @"Select R.Operacion,R.cod_concepto,R.cedula,S.nombre,R.Monto,R.Dias_plazo,R.Tasa_Corriente, R.cuota,R.cod_Contrato
                        ,D.descripcion as ContratoDesc,C.descripcion as ConceptoDesc,R.Registro_Usuario,R.Registro_Fecha,R.Notas
                        from CxC_Cuentas R inner join CxC_Personas S on R.cedula = S.cedula
                        inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto
                        left join CxC_Contratos D on R.cod_Contrato = D.cod_Contrato
                        where R.Autoriza_Fecha is null and R.Estado = 'R' and R.Operacion = @operacion"; 
             
            var resp = DbHelper.ExecuteSingleQuery<CuentasSGTAutorizacionDto>(_portalDB, codEmpresa, query, default, new { operacion });

            

            if (resp.Result is null)
                return DbHelper.CreateErrorResponse<CuentasSGTAutorizacionDto?>("No se encontró la operación solicitada.", -1);

            var dto = resp.Result;
            dto.NotasDetalle = CxCCuentasSGTAutorizacion_Validar(codEmpresa, dto.cedula,dto.Operacion,dto.Monto,dto.cod_concepto);

            dto.Registro_FechaStr = dto.Registro_Fecha.ToString("dd/MM/yyyy");

            return DbHelper.CreateOkResponse<CuentasSGTAutorizacionDto?>(dto);

        }
        /// <summary>
        /// Validaciones Finales 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="operacion"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        public string CxCCuentasSGTAutorizacion_Validar(int codEmpresa, string cedula, int operacion, decimal monto, string cod_concepto)
        {

            string response = "";
 
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                response += PersonaDisponibleValida(codEmpresa, cedula, monto, cod_concepto); 

                response += Operacio_FacturasVerifica(codEmpresa, operacion);
            }

            catch (Exception ex)
            {
                 response="";
            } 

            return response;


        }
        /// <summary>
        /// Consolida Varias Del disponible y Contabilizacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="monto"></param>
        /// <returns></returns>
        private string PersonaDisponibleValida(int codEmpresa, string cedula, decimal monto, string cod_concepto)
        {

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string query = "SELECT dbo.fxCxC_Persona_Disponible_Valida(@cedula, @monto,@cod_concepto );";          
            var dato = conn.QuerySingle<string>(query, new { cedula, monto, cod_concepto });

            return dato;
        }

        /// <summary>
        /// Verifica que no Existan Facturas Duplicadas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        private string Operacio_FacturasVerifica(int codEmpresa, int operacion)
        {
            string respuesta = string.Empty;

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string query = "exec spCxC_Operacion_Facturas_Verifica  @operacion"; 
            var facturas = conn.Query<OperacionFacturasDto>(query, new { operacion }).ToList();


            if (facturas.Count == 0)
                return string.Empty;


            foreach (var item in facturas)
            {
                respuesta +=
                            $"\r\n- Factura No.: {item.cod_factura?.ToString().Trim()}," +
                            $" se encuentra registrada en la Operación: {item.Operacion}";

            }

            return respuesta;
        }

        /// <summary>
        /// Actualiza el registro de la cuenta 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="estado"></param>
        /// <param name="operacion"></param>
        /// <param name="notas"></param>
        /// <returns></returns>
        public ErrorDto CxCCuentasSGTAutorizacion_Actualizar(int codEmpresa, string usuario, string estado, int operacion, string notas)
        {

            const string sqlUpdate = @"     
                UPDATE CxC_Cuentas
                SET Autoriza_Usuario = @usuario,
                    Autoriza_fecha =Null,
                    Autoriza_notas = @notas,
                    Autoriza_Estado =@estado                    
                WHERE Operacion = @operacion;

            ";

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpdate, defaultValue: "",
                parameters: new
                {
                    usuario,
                    notas,
                    estado,
                    operacion,
                });

            if (upsert.Code != 0)
            {
                return DbHelper.ErrorResponse("No fue posible actualizar.");

            }
            var detalle = $"Resolución de la Operación: {operacion} -> Estado : {estado}";

            LogBitacora(codEmpresa, usuario, detalle, MovAplica);


            return DbHelper.CreateOkResponse();
        }



    }
}

using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Clientes;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasControlDB
    {
       private readonly PortalDB _portalDb;
    
       public FrmCrPolizasControlDB(IConfiguration config)
       {
          _portalDb = new PortalDB(config);
       }

        public ErrorDto<PolizaLookupResponseDto> Cr_PolizasControl_Obtener(int CodEmpresa, string CodPoliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT 
                            COD_POLIZA CodPoliza,
                            DESCRIPCION Descripcion,
                            Poliza_General
                        FROM CRD_CATALOGO_POLIZAS
                        WHERE COD_POLIZA = @CodPoliza";

                return conn.QueryFirstOrDefault<PolizaLookupResponseDto>(
                    query,
                    new { Cedula = CodPoliza.Trim() }
                ) ?? new PolizaLookupResponseDto();
            });
        }

        public ErrorDto<PolizaLookupResponseDto?> Cr_PolizasControl_Scroll(
                int codEmpresa,
                string codPolizaActual,
                int direccion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            string sql = direccion == 1
                ? @"
            SELECT TOP 1
                COD_POLIZA CodPoliza,
                DESCRIPCION Descripcion
            FROM CRD_CATALOGO_POLIZAS
            WHERE COD_POLIZA > @CodPolizaActual
            ORDER BY COD_POLIZA ASC"
                : @"
            SELECT TOP 1
                COD_POLIZA CodPoliza,
                DESCRIPCION Descripcion
            FROM CRD_CATALOGO_POLIZAS
            WHERE COD_POLIZA < @CodPolizaActual
            ORDER BY COD_POLIZA DESC";

            var result = connection.QueryFirstOrDefault<PolizaLookupResponseDto>(
                sql,
                new { CodPolizaActual = codPolizaActual });

            return DbHelper.CreateOkResponse<PolizaLookupResponseDto?>(result);
        }

        /// <summary>
        /// Obtiene la lista de cierres de una póliza (equivalente a sbCierreLsw en VB6).
        /// </summary>
        public ErrorDto<List<CrPolizasControlCierreRowDto>> Cr_PolizasControl_Cierres_Lista(
            int CodEmpresa,
            string cod_poliza,
            string tipos)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                SELECT DISTINCT
                    cod_corte,
                    fecha_corte,
                    Tipo,
                    Registro_Usuario,
                    Registro_Fecha
                FROM crd_polizas_cortes
                WHERE cod_poliza = @cod_poliza
                  AND Tipo = @Tipos
                ORDER BY cod_corte DESC";

                return conn.Query<CrPolizasControlCierreRowDto>(
                    query,
                    new
                    {
                        cod_poliza = cod_poliza.Trim(),
                        Tipos = tipos
                    }
                ).ToList();
            });
        }

        /// <summary>
        /// Genera un nuevo cierre de póliza (VB6: btnNuevo_Click).
        /// </summary>
        public ErrorDto Cr_PolizasControl_Nuevo(int CodEmpresa, CrPolizasControlNuevoRequestDto request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (string.IsNullOrWhiteSpace(request.CodPoliza))
                    return DbHelper.ErrorResponse("Debe indicar la póliza.");

                if (string.IsNullOrWhiteSpace(request.Tipo))
                    return DbHelper.ErrorResponse("Debe indicar el tipo de cierre.");

                var fecha = request.FechaCorte?.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                connection.Execute(
                    "spCrdPolizasCierre",
                    new
                    {
                        Tipo = request.Tipo.Trim(),
                        Corte = fecha,
                        Vence = fecha,
                        Usuario = request.Usuario.Trim(),
                        Poliza = request.CodPoliza.Trim(),
                        Actualiza = 0
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return DbHelper.OkResponse($@"Cierre : {request.Tipo} Realizado Satisfactoriamente...");
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al realizar el cierre. Verifique que la póliza exista y que no haya cierres previos para la fecha indicada.");
            }
            
        }

        /// <summary>
        /// Actualiza pólizas (VB6: btnActualizar_Click -> exec spCrdPolizasActualizacion).
        /// </summary>
        public ErrorDto Cr_PolizasControl_Actualizar(int CodEmpresa)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            try
            {
                connection.Execute(
                    "spCrdPolizasActualizacion",
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return DbHelper.OkResponse("Pólizas actualizadas satisfactoriamente.");
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al actualizar la pólizas.");
            }
        }


        /// <summary>
        /// Elimina un cierre de póliza (solo preliminar) desde crd_polizas_cortes (VB6: btnEliminar_Click).
        /// </summary>
        public ErrorDto Cr_PolizasControl_Cierre_Eliminar(
            int CodEmpresa,
            string cod_poliza,
            int cod_corte,
            string Tipo,
            string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza))
                    return DbHelper.ErrorResponse("Debe indicar la póliza.");

                if (cod_corte <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el corte.");

                if (string.IsNullOrWhiteSpace(Tipo))
                    return DbHelper.ErrorResponse("Debe indicar el tipo.");

                Tipo = Tipo.Trim().ToUpperInvariant();

                // Regla VB6: solo puede eliminar cierres preliminares
                if (Tipo != "P")
                    return DbHelper.ErrorResponse("Solo se pueden eliminar cierres preliminares.");

                const string sql = @"
            DELETE FROM crd_polizas_cortes
            WHERE cod_poliza = @cod_poliza
              AND cod_corte  = @cod_corte
              AND Tipo       = @Tipo;";

                var affected = connection.Execute(sql, new
                {
                    cod_poliza = cod_poliza.Trim(),
                    cod_corte,
                    Tipo
                });

                if (affected <= 0)
                    return DbHelper.ErrorResponse("No se encontró el cierre para eliminar.");

                return DbHelper.OkResponse("Cierre eliminado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("No se pudo eliminar la linea: " + ex.Message);
            }

            
        }
    }
}

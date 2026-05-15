using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data.Common;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaAutorizacionesDB
    {
        private readonly PortalDB _portalDb;
        public FrmPreaAutorizacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el Id_Comite de un preanálisis.
        /// </summary>
        public ErrorDto<PreaComiteIdDto> PreaAutorizaciones_ObtenerComite(int codEmpresa, string expediente)
        {
            var sql = "select isnull(Id_comite,0) as Id_Comite from CRD_PREA_PREANALISIS where COD_PREANALISIS = @expediente";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<PreaComiteIdDto>(sql, new { expediente });
                return new ErrorDto<PreaComiteIdDto> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<PreaComiteIdDto> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<PreaComiteIdDto> { Result = null, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Obtiene los miembros de un comité y su asignación para un expediente.
        /// </summary>
        public ErrorDto<List<PreaComiteMiembroDto>> PreaAutorizaciones_ObtenerMiembros(int codEmpresa, int comite, string expediente)
        {
            var sql = @"select M.CEDULA, M.NOMBRE, A.CEDULA as Asignado
                        from CRD_COMITES_AUTORIZADORES CA
                        inner join CRD_COMITES_MIEMBROS M on CA.CEDULA = M.CEDULA
                        left join CRD_PREA_AUTORIZADORES A on CA.CEDULA = A.CEDULA and A.COD_PREANALISIS = @expediente
                        where M.ESTADO = 'A' and CA.ID_COMITE = @comite
                        order by M.NOMBRE";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.Query<PreaComiteMiembroDto>(sql, new { comite, expediente }).AsList();
                return new ErrorDto<List<PreaComiteMiembroDto>> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<List<PreaComiteMiembroDto>> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<List<PreaComiteMiembroDto>> { Result = null, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta un autorizador para un preanálisis.
        /// </summary>
        public ErrorDto<bool> PreaAutorizaciones_Insertar(int codEmpresa, PreaAutorizadorRequestDto request)
        {
            var sql = @"insert CRD_PREA_AUTORIZADORES (COD_PREANALISIS, CEDULA, USUARIO) values (@Expediente, @Cedula, @Usuario)";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var rows = conn.Execute(sql, request);
                return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se insertó" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Elimina un autorizador de un preanálisis.
        /// </summary>
        public ErrorDto<bool> PreaAutorizaciones_Eliminar(int codEmpresa, string expediente, string cedula)
        {
            var sql = @"delete CRD_PREA_AUTORIZADORES where CEDULA = @cedula and COD_PREANALISIS = @expediente";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var rows = conn.Execute(sql, new { cedula, expediente });
                return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se eliminó" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<bool> { Result = false, Code = -1, Description = ex.Message };
            }
        }
    }
}

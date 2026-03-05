using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRazonesFinanzasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 20;
        private const string registra = "Registra - WEB";
        private const string modifica = "Modifica - WEB";

        public FrmCntXRazonesFinanzasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Obtiene la lista de tipos de razones financieras.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de tipos de razones.</returns>
        public ErrorDto<List<CntXRazonesFinanzasDto>> CntXRazonesFinanzas_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select cod_grupo as CodGrupo, descripcion, activa
                from CntX_razones_tipos
                where cod_contabilidad = @codContabilidad
                order by cod_grupo";
            return DbHelper.ExecuteListQuery<CntXRazonesFinanzasDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Valida si existen tipos de razones para la contabilidad dada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>True si existen registros.</returns>
        public ErrorDto<bool> CntXRazonesFinanzas_Existe(int codEmpresa, int codContabilidad)
        {
            var sql = @"select isnull(count(*),0) as Existe from CntX_razones_tipos where cod_contabilidad = @codContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, default, new { codContabilidad }).Result;
            return DbHelper.CreateOkResponse(existe > 0);
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de razón financiera.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del registro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXRazonesFinanzas_Guardar(int codEmpresa, CntXRazonesFinanzasSaveParams param)
        {
            var sqlExist = @"SELECT COUNT(1) FROM CntX_razones_tipos WHERE cod_grupo = @CodGrupo AND cod_contabilidad = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExist, default, new { param.CodGrupo, param.CodContabilidad }).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    insert into CntX_razones_tipos(cod_grupo, cod_contabilidad, descripcion, activa)
                    values(@CodGrupo, @CodContabilidad, @Descripcion, @Activa)";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Fin. Tipo / Grupo : {param.CodGrupo} - {param.Descripcion}", registra);

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    update CntX_razones_tipos
                    set descripcion = @Descripcion,
                        activa = @Activa
                    where cod_contabilidad = @CodContabilidad
                      and cod_grupo = @CodGrupo";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Fin. Tipo / Grupo : {param.CodGrupo} - {param.Descripcion}", modifica);

                return result;
            }
        }

        /// <summary>
        /// Obtiene la lista de razones financieras.
        /// </summary>
        public ErrorDto<List<CntXRazonFinancieraDto>> CntXRazonFinanciera_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select R.cod_razon as CodRazon,
                       R.descripcion,
                       R.resultado,
                       (T.cod_grupo + ' - ' + T.descripcion) as Grupo
                from CntX_Razones_Tipos T
                inner join CntX_Razones R
                    on T.cod_contabilidad = R.cod_contabilidad
                    and T.cod_grupo = R.cod_grupo
                where R.cod_contabilidad = @codContabilidad
                order by T.cod_grupo, R.cod_razon";
            return DbHelper.ExecuteListQuery<CntXRazonFinancieraDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de tipos de razones financieras (solo descripción).
        /// </summary>
        public ErrorDto<List<CntXRazonFinancieraTipoDto>> CntXRazonFinancieraTipos_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select cod_grupo as CodGrupo,
                       (cod_grupo + ' - ' + descripcion) as Descripcion
                from CntX_Razones_Tipos
                where cod_contabilidad = @codContabilidad
                order by cod_grupo";
            return DbHelper.ExecuteListQuery<CntXRazonFinancieraTipoDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una razón financiera.
        /// </summary>
        public ErrorDto<bool> CntXRazonFinanciera_Guardar(int codEmpresa, CntXRazonFinancieraSaveParams param)
        {
            var sqlExist = @"SELECT COUNT(1) FROM CntX_Razones WHERE cod_razon = @CodRazon AND cod_contabilidad = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExist, default, new { param.CodRazon, param.CodContabilidad }).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    insert into CntX_Razones(cod_razon, descripcion, cod_contabilidad, resultado, cod_grupo, notas)
                    values(@CodRazon, @Descripcion, @CodContabilidad, @Resultado, @CodGrupo, '')";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Financiera Id: {param.CodRazon} - {param.Descripcion}", registra);

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    update CntX_Razones
                    set descripcion = @Descripcion,
                        resultado = @Resultado,
                        cod_grupo = @CodGrupo
                    where cod_contabilidad = @CodContabilidad
                      and cod_razon = @CodRazon";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Financiera Id: {param.CodRazon} - {param.Descripcion}", modifica);

                return result;
            }
        }
    }
}

using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasRoeDb
    {
        private readonly PortalDB _portalDB;
        public FrmCajasRoeDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoaTiposIds_Obtener(int cod_empresa)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                            SELECT
                                CAST(tipo_id AS varchar(20)) AS item,
                                RTRIM(descripcion) AS descripcion
                            FROM afi_tipos_ids
                            ORDER BY tipo_id;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoaPaises_Obtener(int cod_empresa)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                            SELECT
                                CAST(cod_pais AS varchar(10)) AS item,
                                RTRIM(descripcion) AS descripcion
                            FROM paises
                            WHERE activo = 1
                            ORDER BY omision DESC, descripcion ASC;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> provincias_por_pais_obtener(
            int cod_empresa,
            string cod_pais)
                {
                    return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                    {
                        const string query = @"
                                SELECT
                                    CAST(provincia AS varchar(10)) AS item,
                                    RTRIM(descripcion) AS descripcion
                                FROM provincias
                                WHERE cod_pais = @cod_pais
                                ORDER BY descripcion;";

                        return conn.Query<DropDownListaGenericaModel>(
                            query,
                            new { cod_pais }
                        ).ToList();
                    });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> cantones_por_provincia_obtener(
                int cod_empresa,
                string provincia)
                    {
                        return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                        {
                            const string query = @"
                                    SELECT
                                        CAST(canton AS varchar(10)) AS item,
                                        RTRIM(descripcion) AS descripcion
                                    FROM cantones
                                    WHERE provincia = @provincia
                                    ORDER BY descripcion;";

                            return conn.Query<DropDownListaGenericaModel>(
                                query,
                                new { provincia }
                            ).ToList();
                        });
        }


        public ErrorDto<List<DropDownListaGenericaModel>> distritos_por_provincia_canton_obtener(
            int cod_empresa,
            string provincia,
            string canton)
                {
                    return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
                    {
                        const string query = @"
                                SELECT
                                    CAST(distrito AS varchar(10)) AS item,
                                    RTRIM(descripcion) AS descripcion
                                FROM distritos
                                WHERE provincia = @provincia
                                  AND canton = @canton
                                ORDER BY descripcion;";

                        return conn.Query<DropDownListaGenericaModel>(
                            query,
                            new { provincia, canton }
                        ).ToList();
                    });
        }


        public ErrorDto<CajasRoeModelDto> cajas_roe_obtener_por_id(
            int cod_empresa,
            int id_roe)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                        SELECT
                            *
                        FROM dbo.vcajas_roe
                        WHERE id_roe = @id_roe;";

                return conn.QueryFirstOrDefault<CajasRoeModelDto>(
                    query,
                    new { id_roe }
                ) ?? new CajasRoeModelDto();
            });
        }


        public ErrorDto<int> cajas_roe_imprime_valida(
                int cod_empresa,
                int id_roe)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                        SELECT
                            dbo.fxCajas_ROE_Imprime_Valida(@id_roe) AS imprime;";

                return conn.QueryFirstOrDefault<int>(
                    query,
                    new { id_roe }
                );
            });
        }

        public ErrorDto<SpResultadoModel> cajas_roe_actualizar(
            int cod_empresa,
             CajasRoeActualizaParamsModel p)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
                    EXEC dbo.spcajas_roe_actualiza
                         @roe
                        ,@tipoiddesc
                        ,@provincia
                        ,@canton
                        ,@distrito
                        ,@direccion
                        ,@telefono
                        ,@fecha_nac
                        ,@tipo_trans
                        ,@tipo_operacion
                        ,@origen_recursos
                        ,@observaciones
                        ,@datos_beneficiario
                        ,@usuario
                        ,@tipo_id
                        ,@pais_id
                        ,@pais
                        ,@provincia_id
                        ,@canton_id
                        ,@distrito_id;";

                return conn.QueryFirstOrDefault<SpResultadoModel>(query, p) ?? new SpResultadoModel();
            });
        }

        public ErrorDto<SpResultadoModel> spcajas_roe_imprime_ejecutar(
                int cod_empresa,
                CajasRoeImprimeParamsModel p)
        {
            return DbHelper.WithConn(_portalDB, cod_empresa, conn =>
            {
                const string query = @"
            EXEC dbo.spcajas_roe_imprime
                 @roe
                ,@usuario;";

                return conn.QuerySingle<SpResultadoModel>(query, p);
            });
        }

    }
}

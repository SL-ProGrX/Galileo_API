
# SonarQube y duplicación

- Considerar SonarQube al revisar cambios; ejecutar o consultar el análisis del código nuevo y modificado cuando la integración esté autenticada.
- Buscar lógica existente antes de copiarla y reutilizar helpers solo cuando conserven la misma semántica, autorización y contrato con procedimientos almacenados.
- Tomar como referencia la separación de responsabilidades de `frmDSB_ColaboradorDB`; evitar duplicar consultas, mapeos y validaciones equivalentes, sin crear abstracciones que oculten diferencias reales.
- Si SonarQube no está disponible, hacer una revisión manual de duplicación e informar que los resultados del análisis no se pudieron confirmar. No declarar resueltos issues o métricas sin repetir el análisis.

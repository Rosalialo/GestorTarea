# Políticas de Combinación de Ramas

Para mantener un flujo de trabajo limpio y ordenado en el proyecto, se definen las siguientes políticas:

## Tipos de combinación permitidos

**Squash merge**: Permitido y recomendado para mantener un historial de commits limpio y resumido.

**Merge commit**: No permitido para evitar historial confuso con múltiples commits de fusión.

**Rebase merge**: No permitido directamente en el repositorio remoto para evitar reescritura de historial compartido.

## Pruebas automáticas
Es obligatorio que todas las pruebas automáticas pasen correctamente antes de aceptar cualquier Pull Request.

## Revisiones de código

Es obligatorio contar con al menos **una revisión y aprobación** de un miembro del equipo (idealmente el líder del proyecto o un desarrollador senior) antes de hacer merge a la rama `main` o `release/*`.

## Reglas adicionales

Las ramas `main` y `release/*` están protegidas. Solo los usuarios con permisos de escritura pueden aprobar y hacer merge.
Los colaboradores externos deberán trabajar siempre mediante Pull Requests y no tienen acceso directo para hacer push a ramas protegidas.

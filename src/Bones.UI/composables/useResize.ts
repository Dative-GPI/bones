import { watch } from 'vue';

export function useResize(
  getElement: () => HTMLElement | null | undefined,
  onResize: () => void
) {
  let resizeObserver: ResizeObserver | null = null;

  watch(
    () => getElement(),
    (newElement, _, onCleanup) => {
      if (newElement && typeof ResizeObserver !== 'undefined') {
        resizeObserver = new ResizeObserver(() => {
          onResize();
        });
        resizeObserver.observe(newElement);

        onCleanup(() => {
          resizeObserver?.disconnect();
          resizeObserver = null;
        });
      }
    },
    { immediate: true }
  );

  return {};
}
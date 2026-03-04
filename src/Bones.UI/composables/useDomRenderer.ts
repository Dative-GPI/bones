import { h, render, getCurrentInstance, onBeforeUnmount, toValue, type Component, type MaybeRefOrGetter, watch } from "vue";

export interface RenderHandle {
  unsubscribe: () => void;
  getElement: (style?: Partial<CSSStyleDeclaration>) => HTMLElement;
}

interface Subscription {
  mountPoint: HTMLElement;
  stopWatching: () => void;
}

function destroySubscription(subscription: Subscription) {
  subscription.stopWatching();
  render(null, subscription.mountPoint);
  subscription.mountPoint.remove();
}

export function useDomRenderer<TProps extends Record<string, any>>(component: Component<TProps>) {

  const instance = getCurrentInstance();
  if (!instance) {
    throw new Error("useDomRenderer must be used inside setup()");
  }

  const appContext = instance.appContext;
  const subscriptions = new Set<Subscription>();

  const subscribe = (getProps: MaybeRefOrGetter<TProps>, style?: Partial<CSSStyleDeclaration>): RenderHandle => {
    const mountPoint = document.createElement("div");

    const stopWatching = watch(
      () => toValue(getProps),
      (props) => {
        const vnode = h(component, props);
        vnode.appContext = appContext;
        render(vnode, mountPoint);
      },
      { immediate: true, deep: true }
    );

    const subscription: Subscription = { mountPoint, stopWatching };
    subscriptions.add(subscription);

    const unsubscribe = () => {
      if (!subscriptions.has(subscription)) {
        throw new Error("This render handle has already been unsubscribed");;
      }
      destroySubscription(subscription);
      subscriptions.delete(subscription);
    };

    const getElement = (newStyle?: Partial<CSSStyleDeclaration>): HTMLElement => {
      if (!subscriptions.has(subscription)) {
        throw new Error("This render handle has already been unsubscribed");
      }
      mountPoint.style.cssText = "";
      Object.assign(mountPoint.style, style ?? {}, newStyle ?? {});
      return mountPoint;
    };

    return { unsubscribe, getElement };
  };

  const unsubscribeAll = () => {
    for (const subscription of subscriptions) {
      destroySubscription(subscription);
    }
    subscriptions.clear();
  };

  onBeforeUnmount(unsubscribeAll);

  return {
    subscribe,
  };
}
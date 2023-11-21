import Vue from 'vue';

declare module 'vue' {
    interface ComponentCustomProperties {
        $tr: (codeLabel: string, defaultLabel: string) => string;
        $pm: {
            some(...permissionCodes: string[]): boolean;
            every(...permissionCodes: string[]): boolean;
            has(permissionCode: string): boolean;
        };
    }
}

declare global {
    interface Window {
      _bonesQueue: any;
    }
  }
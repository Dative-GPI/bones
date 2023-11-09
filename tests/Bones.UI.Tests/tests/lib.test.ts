/**
 * @jest-environment jsdom
 */
import *  as all from '@dative-gpi/bones-ui';

test('basic', () => {
  expect(all).toBeDefined();
});
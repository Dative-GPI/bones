/**
 * @jest-environment jsdom
 */

import *  as all from '@dative-gpi/bones-ui';
import { ServiceFactory } from '@dative-gpi/bones-ui';
import { useTestUser, useTestUsers, TEST_USERS_URL, TEST_USER_URL, useCreateTestUser, useUpdateTestUser, useRemoveTestUser } from '../services/testUserService';
import MockAdapter from 'axios-mock-adapter';

import { buildURL } from '@dative-gpi/bones-ui/tools/buildURL';

const mock = new MockAdapter(ServiceFactory.http);

test('testComposableGet', async () => {
  mock.onGet(TEST_USER_URL("1")).reply(200, { id: "1", label: "test" });
  const { get, entity: user } = useTestUser();
  await get("1");
  expect(user.value?.label).toBe("test");
});

test('testComposableGetMany', async () => {
  mock.onGet(TEST_USERS_URL).reply(200, [{ id: "1", label: "test1" },{ id: "2", label: "test2" }]);
  const { getMany, entities: users } = useTestUsers();
  await getMany();
  expect(users.value?.length).toBe(2);
});

test('testComposableCreate', async () => {
  mock.onPost(TEST_USERS_URL).reply(200,  { id: "1", label: "test" });
  const { create, created } = useCreateTestUser();
  await create({ label: "test" });
  expect(created).toBeTruthy();
});

test('testComposableUpdate', async () => {
  mock.onPost(TEST_USER_URL("1")).reply(200,  { id: "1", label: "updated" });
  const { update, updated } = useUpdateTestUser();
  await update("1", { label: "updated" });
  expect(updated.value?.label).toBe("updated");
});

test('testComposableRemove', async () => {
  mock.onDelete(TEST_USER_URL("1")).reply(200);
  const { remove, removing } = useRemoveTestUser();
  await remove("1");
  expect(removing.value).toBeFalsy();
});


//create and look at the list
test('testComposableComplexeCreate', async () => {
  mock.onGet(TEST_USERS_URL).reply(200, [{ id: "1", label: "test1" },{ id: "2", label: "test2" }]);
  mock.onPost(TEST_USERS_URL).reply(200,  { id: 3, label: "test3" });
  const { getMany, entities: users } = useTestUsers();
  await getMany();
  expect(users.value?.length).toBe(2);
  const { create, created } = useCreateTestUser();
  await create({ label: "test3" });
  expect(created).toBeTruthy();
  expect(users.value?.length).toBe(3);  
});

//update and look at the list
test('testComposableComplexeUpdate', async () => {
  mock.onGet(TEST_USERS_URL).reply(200, [{ id: "1", label: "test1" },{ id: "2", label: "test2" }]);
  mock.onPost(TEST_USER_URL("1")).reply(200,  { id: "1", label: "updated" });
  const { getMany, entities: users } = useTestUsers();
  await getMany();
  expect(users.value?.at(0)?.label).toBe("test1");
  const { update, updated } = useUpdateTestUser();
  await update("1", { label: "updated" });
  expect(users.value?.at(0)?.label).toBe("updated");
});

//remove and look at the list
test('testComposableComplexeRemove', async () => {
  mock.onGet(TEST_USERS_URL).reply(200, [{ id: "1", label: "test1" },{ id: "2", label: "test2" }]);
  mock.onDelete(TEST_USER_URL("1")).reply(200);
  const { getMany, entities: users } = useTestUsers();
  await getMany();
  expect(users.value.length).toBe(2);
  const { remove, removing } = useRemoveTestUser();
  await remove("1");
  expect(users.value.length).toBe(1);
});

//try to getmany with filter then create and see if only filtered are added
test('testComposableComplexeFilter', async () => {
  mock.onGet(buildURL(TEST_USERS_URL, {label: "test3"})).reply(200, [{ id: "1", label: "test1" },{ id: "2", label: "test2" }]);
  const { getMany, entities: users } = useTestUsers();
  await getMany({ label: "test3"});
  expect(users.value?.length).toBe(2);
  const { create, created } = useCreateTestUser();
  mock.onPost(TEST_USERS_URL).reply(200,  { id: "3", label: "filtered" });
  await create({ label: "filtered" });
  expect(created).toBeTruthy();
  expect(users.value?.length).toBe(2);
  mock.onPost(TEST_USERS_URL).reply(200,  { id: "4", label: "test3" });
  await create({ label: "test3" });
  expect(created).toBeTruthy();
  expect(users.value?.length).toBe(3);  
});
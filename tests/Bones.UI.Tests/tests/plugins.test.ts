import { useTranslations } from '@dative-gpi/bones-ui';

describe('Translation plugin', () => {
  const { $tr } = useTranslations();

  it('should return the correct default value with formatted parameter', () => {
    const result = $tr('code', 'default value : {0}m', "72");
    expect(result).toBe('default value : 72m');
  });

  it('should return the correct default value with formatted parameters', () => {
    const result = $tr('code', 'default value : {0}m, {1}°C', "72", 85);
    expect(result).toBe('default value : 72m, 85°C');
  });

  it('should return the correct default value with equals formatted parameters', () => {
    const result = $tr('code', 'default value : {0}m, {1}°C', "72", "72");
    expect(result).toBe('default value : 72m, 72°C');
  });
});


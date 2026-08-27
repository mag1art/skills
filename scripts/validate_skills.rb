#!/usr/bin/env ruby
# frozen_string_literal: true

require "yaml"
require "set"

root = File.expand_path("..", __dir__)
files = Dir.glob(File.join(root, "**", "SKILL.md")).sort
errors = []
names = Set.new

errors << "No SKILL.md files found" if files.empty?

files.each do |path|
  relative = path.delete_prefix("#{root}/")
  content = File.read(path, encoding: "UTF-8")

  errors << "#{relative}: YAML frontmatter must start at line 1" unless content.start_with?("---\n")
  closing = content.index("\n---\n", 4)
  unless closing
    errors << "#{relative}: YAML frontmatter is not closed"
    next
  end

  begin
    metadata = YAML.safe_load(content[4...closing], permitted_classes: [], aliases: false) || {}
  rescue Psych::Exception => e
    errors << "#{relative}: invalid YAML: #{e.message.lines.first.strip}"
    next
  end

  %w[name version description author license tags triggers].each do |key|
    errors << "#{relative}: missing #{key}" unless metadata.key?(key)
  end

  name = metadata["name"]
  errors << "duplicate skill name: #{name}" if name && !names.add?(name)
  errors << "#{relative}: tags must be a non-empty array" unless metadata["tags"].is_a?(Array) && !metadata["tags"].empty?
  errors << "#{relative}: triggers must be a non-empty array" unless metadata["triggers"].is_a?(Array) && !metadata["triggers"].empty?

  Array(metadata.dig("metadata", "hermes", "related_skills")).each do |related|
    related_path = File.join(root, related.to_s, "SKILL.md")
    errors << "#{relative}: related skill not found: #{related}" unless File.file?(related_path)
  end
end

if errors.empty?
  puts "Validated #{files.length} skills successfully."
  exit 0
end

warn errors.map { |error| "ERROR: #{error}" }
exit 1